﻿using System;

namespace Exchange.Server.MVC.Exceptions
{
    public class ProtocolTypeException : Exception
    {
        public override string Message => "Указан не вырный тип протокола: данные не могут быть получены";
    }
}
