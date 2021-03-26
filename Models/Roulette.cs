using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Masivian.Models
{
    public class Roulette
    {
        public string Id { get; set; }
        public string State { get; set; }
        public bool Exists()
        {
            return !string.IsNullOrEmpty(Id);
        }
        public bool IsClosed()
        {
            return State == "Closed";
        }
        public bool ExistAndIsClosed()
        {
            return Exists() && IsClosed();
        }
        public bool DontExistAndIsClosed()
        {
            return !Exists() && IsClosed();
        }        
    }

}
