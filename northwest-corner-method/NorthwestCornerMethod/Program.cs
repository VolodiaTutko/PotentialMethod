using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NorthwestCornerMethod
{
    class Program
    {

        private static int[] _demand;

        private static int[] _supply;

        private static double[,] _costs;

        private static string errorMessage = "Задані параметри задачі не відповідають умовам методу потенціалів";
        private static void Init(string filename)
        {
            string line;
            using (StreamReader file = new StreamReader(filename))
            {
                // 1 рядок розмірність матриці
                line = file.ReadLine();
                if (line == null)
                {
                    throw new Exception();
                }
                var numArr = line.Split();               
                int countSupply = int.Parse(numArr[0]);
                int countDemand = int.Parse(numArr[1]);

                if (countSupply < 0 || countDemand  < 0)
                {
                    throw new Exception();
                }

                // 2 рядок матриця тарифів
                line = file.ReadLine();
                var costsArray = line.Split();
                if (costsArray.Length != countDemand * countSupply)
                {
                    throw new Exception();
                }

               
                List<int> supplyList = new List<int>();
                List<int> demandList = new List<int>();

                // Рядок запасів
                line = file.ReadLine();
                numArr = line.Split();
                for (int i = 0; i < countSupply; i++)
                {
                    supplyList.Add(int.Parse(numArr[i]));
                }

                
                if (supplyList.Count != countSupply)
                {
                    throw new Exception();
                }

               // рядок потреб
                line = file.ReadLine();
                numArr = line.Split();
                for (int i = 0; i < countDemand; i++)
                {
                    demandList.Add(int.Parse(numArr[i]));
                }
                
                if (demandList.Count != countDemand)
                {
                    throw new Exception();
                }
               
                


                // перевірка чи задача закрита, якщо ні то вводимо фіктивні запаси або потреби
                int totalSupply = supplyList.Sum();
                int totalDemand = demandList.Sum();               
                if (totalSupply > totalDemand)
                {
                    
                    demandList.Add(totalSupply - totalDemand);
                } 
                else if (totalDemand > totalSupply)
                {
                    supplyList.Add(totalDemand - totalSupply);
                }
               
                
                _supply = supplyList.ToArray();
                _demand = demandList.ToArray();

               
                _costs = new double[_supply.Length, _demand.Length];
                for (int i = 0; i < countSupply; i++)
                {
                    for (int j = 0; j < countDemand; j++)
                    { 
                        _costs[i, j] = int.Parse(costsArray[i * (countDemand) + j]);
                    }
                }
            }
        }
        private static Shipment[,] NorthWestCornerRule(int[] demand, int[] supply, double[,] costs)
        {
            
            Shipment[,] plan = new Shipment[supply.Length, demand.Length];

            
            int[] currentDemand = (int[])demand.Clone();
            int[] currentSupply = (int[])supply.Clone();
            double[,] currentCosts = (double[,])costs.Clone();
            for (int r = 0, northwest = 0; r < currentSupply.Length; r++)
            {
                for (int c = northwest; c < currentDemand.Length; c++)
                {
                    int quantity = Math.Min(currentSupply[r], currentDemand[c]);

                    
                    if (quantity > 0)
                    {
                        
                        plan[r, c] = new Shipment(quantity, currentCosts[r, c], r, c);

                      
                        currentSupply[r] -= quantity;
                        currentDemand[c] -= quantity;

                       
                        if (currentSupply[r] == 0)
                        {
                            northwest = c;
                            break;
                        }
                    }
                }
            }

           
            return plan;
        }

		private static Shipment[,] MinimumCostMethod(int[] demand, int[] supply, double[,] costs)
		{
			int supplyLen = supply.Length;
			int demandLen = demand.Length;
			Shipment[,] plan = new Shipment[supplyLen, demandLen];

			int[] remainingSupply = (int[])supply.Clone();
			int[] remainingDemand = (int[])demand.Clone();

			while (remainingSupply.Any(s => s > 0) && remainingDemand.Any(d => d > 0))
			{
				double minCost = double.MaxValue;
				int minR = -1;
				int minC = -1;

				for (int r = 0; r < supplyLen; r++)
				{
					for (int c = 0; c < demandLen; c++)
					{
						if (costs[r, c] < minCost && remainingSupply[r] > 0 && remainingDemand[c] > 0)
						{
							minCost = costs[r, c];
							minR = r;
							minC = c;
						}
					}
				}

				if (minR == -1 || minC == -1)
					break;  

				int quantity = Math.Min(remainingSupply[minR], remainingDemand[minC]);
				plan[minR, minC] = new Shipment(quantity, costs[minR, minC], minR, minC);
				remainingSupply[minR] -= quantity;
				remainingDemand[minC] -= quantity;
			}

			return plan;
		}

		public static Shipment[,] FogelMethod(int[] demand, int[] supply, double[,] costs)
		{
			int m = supply.Length;
			int n = demand.Length;
			Shipment[,] plan = new Shipment[m, n];
			bool[] isSupplyFinished = new bool[m];
			bool[] isDemandFinished = new bool[n];
			int[] remainingSupply = (int[])supply.Clone();
			int[] remainingDemand = (int[])demand.Clone();

			while (remainingSupply.Any(s => s > 0) && remainingDemand.Any(d => d > 0))
			{
				int[] rowPenalties = CalculatePenalties(costs, m, n, isSupplyFinished, true);
				int[] colPenalties = CalculatePenalties(costs, m, n, isDemandFinished, false);


				int maxRowPenalty = rowPenalties.Max();
				int maxColPenalty = colPenalties.Max();

				int r, c;
				if (maxRowPenalty >= maxColPenalty)
				{
					r = Array.IndexOf(rowPenalties, maxRowPenalty);
					c = FindMinCostIndex(costs, r, n, isDemandFinished);
				}
				else
				{
					c = Array.IndexOf(colPenalties, maxColPenalty);
					r = FindMinCostIndex(costs, c, m, isSupplyFinished, true);
				}

				int shipmentQuantity = Math.Min(remainingSupply[r], remainingDemand[c]);
				plan[r, c] = new Shipment(shipmentQuantity, costs[r, c], r, c);
				remainingSupply[r] -= shipmentQuantity;
				remainingDemand[c] -= shipmentQuantity;

				if (remainingSupply[r] == 0) isSupplyFinished[r] = true;
				if (remainingDemand[c] == 0) isDemandFinished[c] = true;
			}

			return plan;
		}
		private static int FindMinCostIndex(double[,] costs, int index, int length, bool[] isFinished, bool isCol = false)
		{
			int minIndex = -1;
			double minCost = double.MaxValue;
			for (int i = 0; i < length; i++)
			{
				if (isFinished[i]) continue;
				double cost = isCol ? costs[i, index] : costs[index, i];
				if (cost < minCost)
				{
					minCost = cost;
					minIndex = i;
				}
			}
			return minIndex;
		}

		private static int[] CalculatePenalties(double[,] costs, int m, int n, bool[] isFinished, bool isRow)
		{
			int[] penalties = new int[isRow ? m : n];
			for (int i = 0; i < penalties.Length; i++)
			{
				if (isFinished[i]) continue;

				double min1 = double.MaxValue, min2 = double.MaxValue;
				for (int j = 0; j < (isRow ? n : m); j++)
				{
					double cost = isRow ? costs[i, j] : costs[j, i];
					if (cost < min1) { min2 = min1; min1 = cost; }
					else if (cost < min2) { min2 = cost; }
				}

				penalties[i] = (int)(min2 - min1);
			}

			return penalties;
		}

        private static (string, double) PlanToString(Shipment[,] planMatrix, int[] demand, int[] supply, double[,] costs, string titleMessage, string resultMessage)
        {
            string planString = titleMessage + "\n";
            double totalCosts = 0;


            for (int i = 0; i < demand.Length; i++)
            {
                planString += demand[i] + " ";
            }
            planString += "\n";


            for (int r = 0; r < supply.Length; r++)
            {
                for (int c = 0; c < demand.Length; c++)
                {

                    if (c == 0)
                    {
                        planString += supply[r] + " ";
                    }


                    Shipment s = planMatrix[r, c];


                    if (s != null && s.R == r && s.C == c)
                    {
                        if (s.Quantity == double.Epsilon)
                        {
                            planString += costs[r, c] + " ";
                        }
                        else
                        {

                            planString += s.Quantity + "|" + costs[r, c] + " ";
                        }
                        totalCosts += (s.Quantity * s.CostPerUnit);

                    }
                    else
                    {
                        planString += costs[r, c] + " ";
                    }
                }
                planString += "\n";
            }


            planString += "\n" + resultMessage + " F(x) = " + totalCosts + "\n";

            return (planString, totalCosts);
        }
        private static Shipment[,] PotentialMethod(Shipment[,] planMatrix, int[] demand, int[] supply, double[,] costs)
        {
            double maxReduction = 0;          
            Shipment[] move = null;
            Shipment leaving = null;

            // поч. опорний план
            Shipment[,] resultPlanMatrix = (Shipment[,])planMatrix.Clone();

            FixDegenerateCase(resultPlanMatrix);

            
            for (int r = 0; r < _supply.Length; r++)
            {
                for (int c = 0; c < _demand.Length; c++)
                {
                  
                    if (resultPlanMatrix[r, c] != null)
                    {
                       continue;
                    }

                   
                    Shipment trial = new Shipment(0, _costs[r, c], r, c);
                    Shipment[] path = GetClosedPath(resultPlanMatrix, trial);                    
                    double reduction = 0;
                    double lowestQuantity = int.MaxValue;
                    Shipment leavingCandidate = null;                   
                    bool plus = true;                   
                    foreach (var s in path)
                    {
                       
                        if (plus)
                        {                           
                            reduction += s.CostPerUnit;
                        } 
                        else
                        {
                            
                            reduction -= s.CostPerUnit;

                          if (s.Quantity < lowestQuantity)
                            {
                                
                                leavingCandidate = s;
                                lowestQuantity = s.Quantity;
                            }
                        }
                       plus = !plus;
                    }

                   
                    if (reduction < maxReduction)
                    {
                       
                        move = path;
                        leaving = leavingCandidate;
                        maxReduction = reduction;
                    }
                }
            }
            if (move != null)
            {
                double q = leaving.Quantity;
                bool plus = true;
                foreach (var s in move)
                {
                  s.Quantity += plus ? q : -q;
                  resultPlanMatrix[s.R, s.C] = s.Quantity == 0 ? null : s;
                    plus = !plus;
                }
                resultPlanMatrix = PotentialMethod(resultPlanMatrix, demand, supply, costs);
            }
            return resultPlanMatrix;
        }

        static List<Shipment> PlanToList(Shipment[,] plan)
        {
            List<Shipment> newList = new List<Shipment>();
            foreach (var item in plan)
            {
                if (null != item)
                {
                    newList.Add(item);
                }
            }
            return newList;
        }


      
        static Shipment[] GetClosedPath(Shipment[,] plan, Shipment s)
        {
            
            List<Shipment> path = PlanToList(plan);
            path.Add(s);
            int before;
            do
            {
               
                before = path.Count;
               path.RemoveAll(ship => 
                {                    
                    var nbrs = GetNeighbors(ship, path);
                    return nbrs[0] == null || nbrs[1] == null;
                });
            }
            while (before != path.Count);

            
            Shipment[] stones = path.ToArray();

            Shipment prev = s;
            
            for (int i = 0; i < stones.Length; i++)
            {
               
                stones[i] = prev;
                prev = GetNeighbors(prev, path)[i % 2];
            }

            
            return stones;
        }

       
        static Shipment[] GetNeighbors(Shipment s, List<Shipment> lst)
        {
            Shipment[] nbrs = new Shipment[2];
            foreach (var o in lst)
            {
                if (o != s)
                {
                    if (o.R == s.R && nbrs[0] == null)
                    {
                       
                        nbrs[0] = o;
                    } 
                    else if (o.C == s.C && nbrs[1] == null)
                    {
                        
                        nbrs[1] = o;
                    }
                    
                   
                    if (nbrs[0] != null && nbrs[1] != null)
                    {
                     
                        break;
                    }
                }
            }
            return nbrs;
        }

        
        static Shipment[,] FixDegenerateCase(Shipment[,] plan)
        {
            const double eps = double.Epsilon;
            
           
            Shipment[,] fixedPlan = (Shipment[,])plan.Clone();

           
            if (_supply.Length + _demand.Length - 1 != PlanToList(plan).Count)
            {
                
                for (int r = 0; r < _supply.Length; r++)
                {
                    for (int c = 0; c < _demand.Length; c++)
                    {
                        
                        if (plan[r, c] == null)
                        {
                            
                            Shipment dummy = new Shipment(eps, _costs[r, c], r, c);

                            if (GetClosedPath(plan, dummy).Length == 0)
                            {
                                
                                plan[r, c] = dummy;
                                return fixedPlan;
                            }
                        }
                    }
                }
            }

           
            return fixedPlan;
        }

        static void Main()
        {
            
            var inputFileName = "C:\\Users\\HP\\Desktop\\northwest-corner-method\\NorthwestCornerMethod\\variant20.txt"; 
            var outputFileName = "C:\\Users\\HP\\Desktop\\northwest-corner-method\\NorthwestCornerMethod\\output.txt";

            try
            {
                Init(inputFileName);
                Shipment[,] northWestPlanMatrix = NorthWestCornerRule(_demand, _supply, _costs);
                Shipment[,] minElementPlanMatrix = MinimumCostMethod(_demand, _supply, _costs);
				Shipment[,] fogelPlanMatrix = FogelMethod(_demand, _supply, _costs);



				(string, double) plan1 = PlanToString(northWestPlanMatrix, _demand, _supply, _costs, "Опорний план методом північно-західного кута:", "Значеня");
			    (string, double) plan2 = PlanToString(minElementPlanMatrix, _demand, _supply, _costs, "Опорний план методом мінімального елемента:", "Значення");
			    (string, double) plan3 = PlanToString(fogelPlanMatrix, _demand, _supply, _costs, "Опорний план методом Фогеля:", "Значення");


                string firstPlanString = plan1.Item1;
				string minPlanString = plan2.Item1;
                string fogelPlanString = plan3.Item1;


				

				Shipment[,] optimalPlanMatrix = PotentialMethod(fogelPlanMatrix, _demand, _supply, _costs);
               
                string optimalPlanString = PlanToString(optimalPlanMatrix, _demand, _supply, _costs, "Оптимальний план для цієї задачі:", "Оптимальне значення функції").Item1;

                
                Out(outputFileName, firstPlanString + "\n"+ minPlanString + "\n" + fogelPlanString + "\n" + optimalPlanString);
            }
            catch (Exception)
            {
                 Out(outputFileName, errorMessage);
            }
        }

      
        private static void Out(string fileName, string result)
        {
           
            if (File.Exists(fileName) == false)
            {
                File.Create(fileName);
            }

            using (StreamWriter file = new StreamWriter(fileName, false))
            {
                file.Write(result);
            }
        }
    }
}