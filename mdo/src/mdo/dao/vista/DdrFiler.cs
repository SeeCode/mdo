using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using gov.va.medora.utils;
using gov.va.medora.mdo.exceptions;
using gov.va.medora.mdo.src.mdo;

namespace gov.va.medora.mdo.dao.vista
{
    public class DdrFiler : DdrQuery
    {
        string operation;
        string[] args;

        public DdrFiler(AbstractConnection cxn) : base(cxn) { }

        public string Operation
        {
            get { return operation; }
            set { operation = value; }
        }

        public string[] Args
        {
            get { return args; }
            set { args = value; }
        }

        internal MdoQuery buildRequest()
        {
            if (String.IsNullOrEmpty(this.Operation))
            {
                throw new ArgumentNullException("Must have an operation");
            }
            VistaQuery vq = new VistaQuery("DDR FILER");
            vq.addParameter(vq.LITERAL, Operation);
            DictionaryHashList lst = new DictionaryHashList();
            for (int i = 0; i < Args.Length; i++)
            {
                lst.Add(Convert.ToString(i+1), Args[i]);
            }
            vq.addParameter(vq.LIST, lst);
            return vq;
        }

        public string execute()
        {
            MdoQuery request = buildRequest();
            return this.execute(request);
        }
    }
}
