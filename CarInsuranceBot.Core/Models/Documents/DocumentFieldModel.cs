using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Models.Documents
{
    internal class DocumentFieldModel<TDocument>
    {
        public DocumentFieldModel(string name, Func<string?, bool> valueHandler)
        {
            Name = name;
            ValueHandler = valueHandler;
        }

        public string Name { get; set; }
        public Func<string?, bool> ValueHandler { get; set; }
    }
}
