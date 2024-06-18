namespace NorthwestCornerMethod
{
    public class Shipment
    {
         public double Quantity { get; set; }
        public double CostPerUnit { get; }
        public int R { get; }
        public int C { get; }
        public Shipment(double quantity, double costPerUnit, int r, int c)
        {
            Quantity = quantity;
            CostPerUnit = costPerUnit;
            R = r;
            C = c;
        }
    }
}