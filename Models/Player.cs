using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Masivian.Models
{
    public class Player
    {
        [Range(0, 10000)]
        public float Ammount { get; set; }
        [Range(0, 36)]
        public int Number { get; set; }        
        public bool IsColor { get; set; }
        public string RouletteId { get; set; }
        public string UserId { get; set; }
        public bool Winner { get; set; }
        public double Price { get; set; }
    }   
}
