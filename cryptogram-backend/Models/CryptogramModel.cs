using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cryptogram_backend.Models
{
    public class CryptogramModel
    {
        public int Id { get; set; }
        public String Answer { get; set; }
        public String ScrambledAnswer { get; set; }
        public String ImageName { get; set; }
        public String ContentType { get; set; }
    }
}
