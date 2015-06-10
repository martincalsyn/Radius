using System;
using Microsoft.SPOT;

using IngenuityMicro.Radius.Fonts;
using IngenuityMicro.Radius.Hardware;
using IngenuityMicro.Radius.Core;
using System.Collections;
using System.Threading;
using PervasiveDigital.Diagnostics;
using PervasiveDigital.Utilities;
using NetMF.IO;

namespace IngenuityMicro.Radius.AppHost
{
    public class AppHost : IAppHost
    {
        private Hashtable _apps = new Hashtable();
        private IRadiusApplication _defaultApp = null;
        private IRadiusApplication _activeApp = null;
        private Stack _appStack = new Stack();

        public AppHost()
        {
        }

        public void AddApplication(RadiusApplication app)
        {
            AddApplication(app, false);
        }

        public void AddApplication(RadiusApplication app, bool isDefaultApp)
        {
            if (_apps.Contains(app.UniqueName))
                _apps.Remove(app.UniqueName);
            _apps.Add(app.UniqueName, app);
            if (isDefaultApp)
                _defaultApp = app;
            app.Initialize(this);
            if (isDefaultApp)
                SwitchTo(app);
        }

        public RadiusApplication FindApplication(string id)
        {
            if (_apps.Contains(id))
                return (RadiusApplication)_apps[id];
            else
                return null;
        }

        public void LaunchMainMenu()
        {
            var app = FindApplication("MainMenu");
            if (app != null)
                PushAndSwitchTo(app);
        }

        public void PushAndSwitchTo(RadiusApplication app)
        {
            if (_activeApp != app)
            {
                _appStack.Push(_activeApp);
                SwitchTo(app);
            }
        }

        public void PopAndSwitch()
        {
            if (_appStack.Count == 0)
                return;

            var returnTo = (RadiusApplication)_appStack.Pop();
            SwitchTo(returnTo);
        }

        public void SwitchTo(IRadiusApplication app)
        {
            if (_activeApp != app)
            {
                if (_activeApp != null)
                    _activeApp.NavigateAway();
                _activeApp = app;
                _activeApp.NavigateTo();
            }
        }

        public IRadiusApplication ActiveApp
        {
            get { return _activeApp; }
        }


        public void Run()
        {
            AddApplication(new AppHostApp());
            Thread.Sleep(Timeout.Infinite);
        }

        public void SerialDataReceived(string msg)
        {
            try
            {
                var jsonMsg = Json.Deserialize(msg);

                if (jsonMsg is Hashtable)
                {
                    var htMsg = (Hashtable)jsonMsg;
                    var target = (string)htMsg["app"];
                    var msgId = (int)htMsg["msgid"];
                    var method = (string)htMsg["method"];
                    var parms = (Hashtable)htMsg["parms"];

                    if (_apps.Contains(target))
                    {
                        ((RadiusApplication)_apps[target]).HandleAppMessage(msgId, method, parms);
                    }

                }
            }
            catch (Exception ex)
            {
                // all exceptions here are ignored - might be parsing errors or app errors, but either way, the show must go on
            }
        }

        public void Send(IRadiusMessage msg)
        {
            var text = msg.Serialize();
            var channel = (IPeerChannel)DiContainer.Instance.Resolve(typeof(IPeerChannel));
            if (channel!=null)
                channel.Send(text);
        }

        public string[] GetInstalledApps()
        {
            var len = _apps.Count;
            string[] result = new string[len];

            int i = 0;
            foreach (var item in _apps.Values)
            {
                result[i] = ((RadiusApplication)item).UniqueName;
                ++i;
            }

            return result;
        }
    }
}
