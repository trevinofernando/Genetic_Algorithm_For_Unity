using System;
using Unity;

public class DNA<T>
{
	public T[] Genes { get; private set; }
	public float Fitness { get; private set; }

	private Random random;
	private Func<int, T> getRandomGene;
	private Func<int, float> fitnessFunction;

    private int crossoverType;

	public DNA(int size, Random random, Func<int, T> getRandomGene, Func<int, float> fitnessFunction, CrossoverTypes crossoverType, bool shouldInitGenes = true)
	{
		Genes = new T[size];
		this.random = random;
		this.getRandomGene = getRandomGene;
		this.fitnessFunction = fitnessFunction;
        this.crossoverType = (int)crossoverType;


        if (shouldInitGenes)
		{
			for (int i = 0; i < Genes.Length; i++)
			{
				Genes[i] = getRandomGene(i);
			}
		}
	}

	public float CalculateFitness(int index)
	{
		Fitness = fitnessFunction(index);
		return Fitness;
	}

    public DNA<T> Crossover(DNA<T> otherParent)
	{
		DNA<T> child = new DNA<T>(Genes.Length, random, getRandomGene, fitnessFunction, (CrossoverTypes)crossoverType, shouldInitGenes: false);
        int xoverPoint1;
        int xoverPoint2;

        switch ((CrossoverTypes)crossoverType)
        {
            case CrossoverTypes.Uniform:
		        
		        for (int i = 0; i < Genes.Length; i++)
		        {
			        child.Genes[i] = random.NextDouble() < 0.5 ? Genes[i] : otherParent.Genes[i];
		        }
                break;

            case CrossoverTypes.OnePoint:

                xoverPoint1 = random.Next(Genes.Length);

                for (int i = 0; i < Genes.Length; i++)
                {
                    child.Genes[i] = (i < xoverPoint1) ? Genes[i] : otherParent.Genes[i];
                }
                break;

            case CrossoverTypes.TwoPoint:

                xoverPoint1 = random.Next(Genes.Length);
                do
                {
                    xoverPoint2 = random.Next(Genes.Length);
                } while (xoverPoint1 == xoverPoint2);
                
                if(xoverPoint1 < xoverPoint2)
                {
                    int tmp = xoverPoint1;
                    xoverPoint1 = xoverPoint2;
                    xoverPoint2 = tmp;
                }

                for (int i = 0; i < Genes.Length; i++)
                {
                    child.Genes[i] = (i < xoverPoint1 || i > xoverPoint2) ? Genes[i] : otherParent.Genes[i];
                }
                break;

            case CrossoverTypes.UniformAverage:
                if (Genes[0].IsNumericType())
                {
                    double average;
                    for (int i = 0; i < Genes.Length; i++)
                    {
                        average = ((Double)(object)Genes[i] + (Double)(object)otherParent.Genes[i]) / 2;
                        child.Genes[i] = (random.NextDouble() < 0.5) ? Genes[i] : (T)(object) (average);
                    }
                }
                else
                {
                    Console.WriteLine("Can't compute Average of a non-numeric variable");
                }
                break;
            case CrossoverTypes.UniformWeightedAverage:
                if (Genes[0].IsNumericType())
                {
                    double weightedAverage;
                    for (int i = 0; i < Genes.Length; i++)
                    {
                        weightedAverage = ((Double)(object)Genes[i] * this.Fitness + (Double)(object)otherParent.Genes[i] * otherParent.Fitness) / (this.Fitness + otherParent.Fitness);
                        child.Genes[i] = (random.NextDouble() < 0.5) ? Genes[i] : (T)(object)(weightedAverage);
                    }
                }
                else
                {
                    Console.WriteLine("Can't compute Average of a non-numeric variable");
                }
                break;
            default:
                Console.WriteLine("Unkonwn Crossover Type");
                break;
        }

		return child;
	}

    public DNA<T> Crossover()
    {
        DNA<T> child = new DNA<T>(Genes.Length, random, getRandomGene, fitnessFunction, (CrossoverTypes)crossoverType, shouldInitGenes: false);
        for (int i = 0; i < Genes.Length; i++)
        {
            child.Genes[i] = Genes[i];
        }
        return child;
    }

    public void Mutate(float mutationRate)
	{
		for (int i = 0; i < Genes.Length; i++)
		{
			if (random.NextDouble() < mutationRate)
			{
				Genes[i] = getRandomGene(i);
			}
		}
	}

}