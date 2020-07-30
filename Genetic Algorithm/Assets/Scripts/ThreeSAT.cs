using System;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public enum DNF_Files
{
    uf20 = 20,
    uf50 = 50,
    uf75 = 75,
    uf100 = 100,
    uf125 = 125,
    uf175 = 175,
    uf200 = 200
}

public class ThreeSAT : MonoBehaviour
{
    [Header("Genetic Algorithm")]
    [NonSerialized] float maxFitness = float.PositiveInfinity;
    [NonSerialized] int dnaSize = 1;

    [SerializeField] DNF_Files proplem = DNF_Files.uf20;
    [SerializeField] int populationSize = 200;
    [SerializeField] CrossoverTypes crossoverType = CrossoverTypes.Uniform;
    [SerializeField] SelectionTypes selectionType = SelectionTypes.Proportional;
    [SerializeField] [Range(0, 1)] float mutationRate = 0.01f;
    [SerializeField] [Range(0, 1)] float crossoverRate = 1f;
    [SerializeField] float elitism = 5;
    [SerializeField] int generationToSave = 10;

    [Header("Other")]
    [SerializeField] int numCharsPerText = 15000;
    [SerializeField] Text bestSolution;
    [SerializeField] Text bestText;
    [SerializeField] Text bestFitnessText;
    [SerializeField] Text numGenerationsText;
    [SerializeField] Transform populationTextParent;
    [SerializeField] Text textPrefab;

    private GeneticAlgorithm<int> ga;
    private System.Random random;
    [NonSerialized] public static int[] CNF;
    [NonSerialized] public static int numVar;
    [NonSerialized] public static int numClauses;

    private string fullPath;

    void Start()
    {

        random = new System.Random();
        SATReader();
        ga = new GeneticAlgorithm<int>(populationSize, dnaSize, random, GetRandomGene, FitnessFunction, elitism, selectionType, crossoverType, crossoverRate, mutationRate);

        fullPath = Application.persistentDataPath + "/" + "GeneticSave";
        ga.LoadGeneration(fullPath);
    }

    void Update()
    {
        ga.NewGeneration();

        UpdateText(ga.BestGenes, ga.BestFitness / maxFitness, ga.Generation, ga.Population.Count, (j) => ga.Population[j].Genes);

        if (ga.Generation % generationToSave == 0)
        {
            //ga.SaveGeneration(fullPath);
            //this.enable = false;
        }

        if (ga.BestFitness >= maxFitness)
        {
            this.enabled = false;
        }

    }

    private int GetRandomGene(int geneIndex)
    {
        //Here you need to find out how to generate a random gene
        //Think range of values for each gene
        //Change function type as needed
        return random.NextDouble() < 0.5 ? 0 : 1;
    }

    private float FitnessFunction(int index)
    {

        DNA<int> dna = ga.Population[index];

        float numSatisfied = 0;
        for (int i = 0; i < CNF.Length; i += 3)
        {
            if ((ga.Population[index].Genes[Math.Abs(CNF[i]) - 1] - ((Math.Sign(CNF[i]) + 1) / 2)) == 0)
            {
                numSatisfied++;
            }
            else if ((ga.Population[index].Genes[Math.Abs(CNF[i + 1]) - 1] - ((Math.Sign(CNF[i + 1]) + 1) / 2)) == 0)
            {
                numSatisfied++;
            }
            else if ((ga.Population[index].Genes[Math.Abs(CNF[i + 2]) - 1] - ((Math.Sign(CNF[i + 2]) + 1) / 2)) == 0)
            {
                numSatisfied++;
            }
        }

        //numSatisfied /= maxFitness;

        //numSatisfied = (Mathf.Pow(5, numSatisfied) - 1) / (5 - 1);

        return numSatisfied;
    }


    [MenuItem("Tools/Read file")]
    private void SATReader()
    {
        string path = "Assets/Resources/uf" + (int)proplem + "-01.txt";

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        String line;
        int clauseIndex = 0;

        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.Length == 0)
            {
                continue;
            }
            switch (line[0])
            {
                case 'p':
                    String[] token = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    numVar = int.Parse(token[2]);
                    numClauses = int.Parse(token[3]);
                    CNF = new int[3 * numClauses];
                    break;
                case 'c':
                case '%':
                case '0':
                    break;

                default:
                    String[] vars = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    CNF[clauseIndex++] = int.Parse(vars[0]);
                    CNF[clauseIndex++] = int.Parse(vars[1]);
                    CNF[clauseIndex++] = int.Parse(vars[2]);
                    break;
            }
        }
        dnaSize = numVar;
        maxFitness = numClauses;
        reader.Close();

    }









    private int numCharsPerTextObj;
    private List<Text> textList = new List<Text>();

    void Awake()
    {
        numCharsPerTextObj = numCharsPerText / 2;
        if (numCharsPerTextObj > populationSize) numCharsPerTextObj = populationSize;

        int numTextObjects = Mathf.CeilToInt((float)populationSize / numCharsPerTextObj);

        for (int i = 0; i < numTextObjects; i++)
        {
            textList.Add(Instantiate(textPrefab, populationTextParent));
        }
    }

    private void UpdateText(int[] bestGenes, float bestFitness, int generation, int populationSize, Func<int, int[]> getGenes)
    {
        bestSolution.text = ToSolution(bestGenes);
        bestText.text = IntArrayToString(bestGenes);
        bestFitnessText.text = bestFitness.ToString();

        numGenerationsText.text = generation.ToString();

        for (int i = 0; i < textList.Count; i++)
        {
            string sb = "";
            int endIndex = i == textList.Count - 1 ? populationSize : (i + 1) * numCharsPerTextObj;
            for (int j = i * numCharsPerTextObj; j < endIndex; j++)
            {
                foreach (var c in getGenes(j))
                {
                    sb += c.ToString();
                }
                if (j < endIndex - 1) sb += "\n";
            }

            textList[i].text = sb;
        }
    }

    private string IntArrayToString(int[] intArray)
    {
        string sb = "";
        foreach (var c in intArray)
        {
            sb += c.ToString();
        }

        return sb;
    }

    private string ToSolution(int[] intArray)
    {

        string solution = "";
        for (int i = 0; i < CNF.Length; i += 3)
        {
            if ((intArray[Math.Abs(CNF[i]) - 1] - ((Math.Sign(CNF[i]) + 1) / 2)) == 0)
            {
                solution += "T";
            }
            else if ((intArray[Math.Abs(CNF[i + 1]) - 1] - ((Math.Sign(CNF[i + 1]) + 1) / 2)) == 0)
            {
                solution += "T";
            }
            else if ((intArray[Math.Abs(CNF[i + 2]) - 1] - ((Math.Sign(CNF[i + 2]) + 1) / 2)) == 0)
            {
                solution += "T";
            }
            else
            {
                solution += "F";
            }
        }
        return solution;
    }

}

