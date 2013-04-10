using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gov.va.medora.mdo.exceptions;
using gov.va.medora.utils;

namespace gov.va.medora.mdo.dao.vista
{
    public class VistaCrudDao
    {
        VistaConnection _cxn;

        public VistaCrudDao(AbstractConnection cxn)
        {
            _cxn = (VistaConnection)cxn;
        }

        #region Delete

        public void delete(String recordIen, String vistaFile)
        {
            DdrFiler request = buildDeleteRequest(recordIen, vistaFile);
            String response = request.execute();
            toCreateUpdateDeleteRecordResponse(response);
        }

        /// <summary>
        /// Query to delete a record from a file
        /// </summary>
        /// <param name="recordIen">The entry IEN. Can be a subfile (IENS string needs to be built correctly)</param>
        /// <param name="vistaFile">The Vista file from which to delete a record</param>
        /// <returns></returns>
        internal DdrFiler buildDeleteRequest(String recordIen, String vistaFile)
        {
            DdrFiler query = new DdrFiler(_cxn);
            query.Operation = "EDIT";
            query.Args = new string[]
            {
                vistaFile + "^.01^" + recordIen + ",^@" // per API docs, setting .01 field to "@" deletes record
            };
            return query;

        }

        #endregion

        #region Create

        /// <summary>
        /// Create a new record entry in a Vista file
        /// </summary>
        /// <param name="fieldsAndValues">The field number and value dictionary</param>
        /// <param name="vistaFile">The Vista file number</param>
        /// <param name="iens">If creating a record in a subfile, the IENS string of the parent record</param>
        /// <returns>The IEN of the new record</returns>
        public String create(Dictionary<String, String> fieldsAndValues, String vistaFile, String iens = null)
        {
            DdrFiler request = buildCreateRequest(fieldsAndValues, vistaFile, iens);
            String response = request.execute();
            return toCreateUpdateDeleteRecordResponse(response);
        }

        internal String toCreateUpdateDeleteRecordResponse(string response)
        {
            if (String.IsNullOrEmpty(response))
            {
                throw new MdoException("An empty response was received but is invalid for this operation");
            }

            String[] pieces = StringUtils.split(response, StringUtils.CRLF);

            if (pieces.Length > 1 && pieces[1].Contains("BEGIN_diERRORS"))
            {
                throw new MdoException(response);
            }

            if (pieces[0].Contains("[Data]") && pieces.Length > 1) //sample create valid response: "[Data]\r\n+1,^2\r\n" <- 2 is IEN for new record
            {
                Int32 startIdx = pieces[1].IndexOf('^');
                return startIdx > 0 ? pieces[1].Substring(startIdx + 1) : "";
            }
            else // "[Data]" response means everything was ok
            {
                return "OK";
            }
        }

        internal DdrFiler buildCreateRequest(Dictionary<String, String> fieldsAndValues, String vistaFile, String iens = null)
        {
            DdrFiler ddr = new DdrFiler(_cxn);
            ddr.Operation = "ADD";

            int index = 0;
            ddr.Args = new String[fieldsAndValues.Count];
            foreach (String key in fieldsAndValues.Keys)
            {
                if (String.IsNullOrEmpty(iens))
                {
                    ddr.Args[index++] = vistaFile + "^" + key + "^+1,^" + fieldsAndValues[key]; // e.g. [0]: 2^.01^+1,PATIENT,NEW  [1]: 2^.09^+1,^222113333
                }
                else
                {
                    ddr.Args[index++] = vistaFile + "^" + key + "^+1," + iens + ",^" + fieldsAndValues[key]; // e.g. [0]: 2^.01^+1,PATIENT,NEW  [1]: 2^.09^+1,^222113333
                }
            }

            return ddr;
        }

        #endregion

        #region Read

        /// <summary>
        /// Returns a dictionary of field numbers and values
        /// </summary>
        /// <param name="recordIen">The IEN in the Vista file</param>
        /// <param name="fields">Separate fields with a semicolon - e.g.: .01;.02;9  Leave blank to retrieve all fields</param>
        /// <param name="vistaFile">The Vista file number</param>
        /// <returns>Dictionary<String, String></returns>
        public Dictionary<String, String> read(String recordIen, String fields, String vistaFile)
        {
            DdrGetsEntry ddr = new DdrGetsEntry(_cxn);
            ddr.Fields = String.IsNullOrEmpty(fields) ? "*" : fields;
            ddr.File = vistaFile;
            ddr.Flags = "I";
            ddr.Iens = String.Concat(recordIen, ",");
            String[] results = ddr.execute();
            return ddr.convertToFieldValueDictionary(results);
        }

        internal DdrGetsEntry buildReadRequest(String recordIen, String fields, String vistaFile)
        {
            DdrGetsEntry ddr = new DdrGetsEntry(_cxn);
            ddr.Fields = String.IsNullOrEmpty(fields) ? "*" : fields;
            ddr.File = vistaFile;
            ddr.Flags = "I";
            ddr.Iens = String.Concat(recordIen, ",");
            return ddr;
        }

        #endregion

        #region Update

        public void update(Dictionary<String, String> fieldsAndValues, String ien, String vistaFile)
        {
            DdrFiler request = buildUpdateRequest(fieldsAndValues, ien, vistaFile);
            String response = request.execute();
            toCreateUpdateDeleteRecordResponse(response); // should throw exception on failure
        }

        internal DdrFiler buildUpdateRequest(Dictionary<String, String> fieldsAndValues, String ien, String vistaFile)
        {
            DdrFiler ddr = new DdrFiler(_cxn);
            ddr.Operation = "UPDATE";

            int index = 0;
            ddr.Args = new String[fieldsAndValues.Count];
            foreach (String key in fieldsAndValues.Keys)
            {
                ddr.Args[index++] = vistaFile + "^" + key + "^" + ien + ",^" + fieldsAndValues[key]; // e.g. [0]: 2^.01^5,^PATIENT,NEW  [1]: 2^.09^5,^222113333
            }

            return ddr;
        }


        #endregion
    }
}
