using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsefulBits.Web.Demo.Services;

namespace UsefulBits.Web.Demo.Areas.Area51.Services
{
  public class Area51CustomService : ICustomService
  {
    public string Name
    {
      get
      {
        return "Area51CustomService";
      }
    }
  }
}
