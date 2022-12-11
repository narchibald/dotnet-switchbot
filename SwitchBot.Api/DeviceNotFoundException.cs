namespace SwitchBot.Api
{
    using System;

    public class DeviceNotFoundException : Exception
    {
        public DeviceNotFoundException(string deviceId)
            : base()
        {
            this.DeviceId = deviceId;
        }

        public DeviceNotFoundException(string deviceId, string message)
            : base(message)
        {
            this.DeviceId = deviceId;
        }

        public DeviceNotFoundException(string deviceId, string message, Exception innerException)
            : base(message, innerException)
        {
            this.DeviceId = deviceId;
        }

        public string DeviceId { get; }
    }
}