/**************************************************************************************/
//This is sample code provided under the Microsoft Limited Public License.
// Copyright (c) Microsoft Corporation. All rights reserved 
/**************************************************************************************/
using System;

namespace Microsoft.Phone.AccessoryManager.AbstractionLayer
{
    public interface IPhoneNotificationTriggerDetails : IAccessoryNotificationTriggerDetails
    {
        IPhoneCallDetails CallDetails { get; }

        Guid PhoneLineChangedId { get; }

        PhoneNotificationType PhoneNotificationType { get; }
    }
}