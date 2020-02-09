using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocketServer
{
    public class MainProgram
    {        
        public static void Main(string[] args)
        {
            WebSocketServerTest WSServerTest = new WebSocketServerTest();
            WSServerTest.Start();
        }
    }
}
