using System;
using Microsoft.SPOT;

namespace IngenuityMicro.Radius.Core
{
    public interface IRadiusMessage
    {
        string Serialize();
    }
}