using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsefulBits.Web.Demo.Services
{
  public class RootCustomService : ICustomService
  {
    public string Name
    {
      get
      {
        return "RootCustomService";
      }
    }
  }
}
