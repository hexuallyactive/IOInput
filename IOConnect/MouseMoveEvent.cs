using System;
namespace IOConnect
{
    public class MouseMoveEvent
    {
		public string uid { get; set; }
		public double elapsed { get; set; }
		public double x { get; set; }
		public double y { get; set; }
    }
}
