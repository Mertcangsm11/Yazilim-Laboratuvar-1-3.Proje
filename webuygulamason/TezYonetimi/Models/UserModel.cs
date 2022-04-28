using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TezYonetimi.Models
{
    public class UserModel
    {
        public int ID { get; set; }
        public int GROUPID { get; set; }
        public string EMAIL { get; set; }
        public string PASSWORD { get; set; }
        public string FULLNAME { get; set; }
    }
}
