using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TezYonetimi.Models
{
    public class GroupModel
    {
        public int ID { get; set; }
        public int GROUPID { get; set; }

        public string GROUPNAME { get; set; }
        public decimal GROUPLIMIT { get; set; }
    }
}
