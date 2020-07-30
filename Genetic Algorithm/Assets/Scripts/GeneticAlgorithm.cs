using System;
using System.Collections.Generic;

public enum CrossoverTypes
{
    Uniform = 0,
    OnePoint = 1,
    TwoPoint = 2,
    UniformAverage = 3,
    UniformWeightedAverage = 4
}

public enum SelectionTypes
{
    Proportional = 0,
    Rank = 1,
    Tournament = 2,
    Random = 3
}

public class GeneticAlgorithm<T>
{
	public List<DNA<T>> Population { get; private set; }
	public int Generation { get; private set; }
	public float BestFitness { get; private set; }
	public T[] BestGenes { get; private set; }

	public int Elitism;
	public CrossoverTypes crossoverType;
    public SelectionTypes selectionType = SelectionTypes.Proportional;
    public float MutationRate = 0.01f;
    public float CrossoverRate = 1f;
	
	private List<DNA<T>> newPopulation;
	private Random random;
	private float fitnessSum;
	private int dnaSize;
	private Func<int, T> getRandomGene;
	private Func<int, float> fitnessFunction;

	public GeneticAlgorithm(int populationSize, int dnaSize, Random random, Func<int, T> getRandomGene, Func<int, float> fitnessFunction,
		float elitism = 1, SelectionTypes selectionType = 0, CrossoverTypes crossoverType = 0, float crossoverRate = 1f, float mutationRate = 0.01f)
	{
		Population = new List<DNA<T>>(populationSize);
		newPopulation = new List<DNA<T>>(populationSize);
		Generation = 1;
		Elitism = Math.Max(0, elitism) < 1 ? (int) elitism * populationSize : (int)elitism; //If less than 1 treat it as a percentage of the population
		MutationRate = mutationRate;
        CrossoverRate = crossoverRate;
		this.random = random;
		this.dnaSize = dnaSize;
		this.getRandomGene = getRandomGene;
		this.selectionType = selectionType;
		this.crossoverType = crossoverType;
		this.fitnessFunction = fitnessFunction;

		BestGenes = new T[dnaSize];

		for (int i = 0; i < populationSize; i++)
		{
			Population.Add(new DNA<T>(dnaSize, random, getRandomGene, fitnessFunction, crossoverType, shouldInitGenes: true));
		}
	}

	public void NewGeneration(int numNewDNA = 0, bool crossoverNewDNA = false)
	{
		int finalCount = Population.Count + numNewDNA;

		if (finalCount <= 0) {
			return;
		}

		if (Population.Count > 0) {
			CalculateFitness();
			Population.Sort(CompareDNA);
		}
		newPopulation.Clear();

		for (int i = 0; i < Population.Count; i++)
		{
			if (i < Elitism && i < Population.Count)
			{
				newPopulation.Add(Population[i]);
			}
			else if (i < Population.Count || crossoverNewDNA)
			{
				DNA<T> parent1 = ChooseParent();
				DNA<T> parent2 = ChooseParent();

                DNA<T> child;
                if (random.NextDouble() < CrossoverRate)
                {
                    child = parent1.Crossover(parent2);
                }
                else
                {
                    child = parent1.Crossover(); //asexual reproduction
                }

				child.Mutate(MutationRate);

				newPopulation.Add(child);
			}
			else
			{
				newPopulation.Add(new DNA<T>(dnaSize, random, getRandomGene, fitnessFunction, crossoverType, shouldInitGenes: true));
			}
		}

		List<DNA<T>> tmpList = Population;
		Population = newPopulation;
		newPopulation = tmpList;

		Generation++;
	}

    public void SaveGeneration(string filePath){
        GeneticSaveData<T> save = new GeneticSaveData<T>{
            Generation = Generation,
            PopulationGenes = new List<T[]>(Population.Count)
        };

        for(int i = 0; i < Population.Count; i++){
            save.PopulationGenes.Add(new T[dnaSize]);
            Array.Copy(Population[i].Genes, save.PopulationGenes[i],dnaSize);
        }

        FileReadWrite.WriteToBinaryFile(filePath, save);
    }

    public bool LoadGeneration(string filePath){
        if(!System.IO.File.Exists(filePath)){
            return false;
        }
        GeneticSaveData<T> save = FileReadWrite.ReadFromBinaryFile<GeneticSaveData<T>>(filePath);
        Generation = save.Generation;
        for(int i = 0; i < save.PopulationGenes.Count; i++){
            if(i >= Population.Count){
                Population.Add(new DNA<T>(dnaSize, random, getRandomGene, fitnessFunction, crossoverType, shouldInitGenes: false));
            }
            Array.Copy(save.PopulationGenes[i], Population[i].Genes, dnaSize);
        }
        return true;
    }
	
	private int CompareDNA(DNA<T> a, DNA<T> b)
	{
		if (a.Fitness > b.Fitness) {
			return -1;
		} else if (a.Fitness < b.Fitness) {
			return 1;
		} else {
			return 0;
		}
	}

	private void CalculateFitness()
	{
		fitnessSum = 0;
		DNA<T> best = Population[0];

		for (int i = 0; i < Population.Count; i++)
		{
			fitnessSum += Population[i].CalculateFitness(i);

			if (Population[i].Fitness > best.Fitness)
			{
				best = Population[i];
			}
		}

		BestFitness = best.Fitness;
		best.Genes.CopyTo(BestGenes, 0);
	}

	private DNA<T> ChooseParent()
	{
        double randomNumber = random.NextDouble();
        double rWheel = 0;

        switch ((SelectionTypes)selectionType)
        {
            case SelectionTypes.Proportional:
                randomNumber *= fitnessSum;

                for (int i = 0; i < Population.Count; i++)
                {
                    if (randomNumber < Population[i].Fitness)
                    {
                        return Population[i];
                    }

                    randomNumber -= Population[i].Fitness;
                }
                break;

            case SelectionTypes.Tournament:
                int temp;
                int tournamentSize = 4;
                float selectionPressure = 0.6f;
                int[] candidate = new int[tournamentSize];

                for (int i = 0; i < tournamentSize; ++i)
                    candidate[i] = (int)(random.NextDouble() * Population.Count);

                for (int i = tournamentSize - 1; i > 0; i--)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (Population[candidate[j]].Fitness > Population[candidate[j + 1]].Fitness)
                        {
                            temp = candidate[j];
                            candidate[j] = candidate[j + 1];
                            candidate[j + 1] = temp;
                        }
                    }
                }
                for (int i = tournamentSize - 1; i > 0; i--)
                    if (random.NextDouble() < selectionPressure)
                        return Population[candidate[i]];
                return Population[candidate[0]];

            case SelectionTypes.Rank:

                Population.Sort(CompareDNA);
                Population.Reverse();

                int n = (int)(randomNumber * ((Population.Count * (Population.Count + 1)) / 2)); //random number in the range of 0 and the sum of all indexes

                for (int i = 0; i < Population.Count; i++)
                {
                    rWheel = rWheel + i + 1;
                    if (n < rWheel)
                        return Population[i];
                }
                break;
            case SelectionTypes.Random:
                return Population[(int)(randomNumber * Population.Count)];
            default:
                Console.WriteLine("Unkonwn Selection Type");
                return null;
        }

        return null;
	}
}

