using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gov.va.medora.mdo.dao.vista
{
    public class CrossRef
    {
        public string Name { get; set; }
        public string FieldNumber { get; set; }
        public string FieldName { get; set; }
        public VistaFile File { get; set; }
        public string DD { get; set; }
    }
}
