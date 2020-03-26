using System;
using System.Collections.Generic;
using System.Text;

namespace DdnsSDK.Interface
{
    public interface IDdnsService
    {
        bool Update();

        bool Insert();
    }
}
