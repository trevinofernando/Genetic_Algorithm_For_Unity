using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class OneMax : MonoBehaviour
{
    [Header("Genetic Algorithm")]
    [SerializeField] int targetStringSize = 100;
    [SerializeField] string validCharacters = "01";
    [SerializeField] int populationSize = 200;
    [SerializeField] CrossoverTypes crossoverType = CrossoverTypes.Uniform;
    [SerializeField] SelectionTypes selectionType = SelectionTypes.Proportional;
    [SerializeField] [Range(0, 1)] float mutationRate = 0.01f;
    [SerializeField] [Range(0, 1)] float crossoverRate = 1f;
    [SerializeField] float elitism = 5;
    [SerializeField] int generationToSave = 10;

    [Header("Other")]
    [SerializeField] int numCharsPerText = 15000;
    [SerializeField] Text targetText;
    [SerializeField] Text bestText;
    [SerializeField] Text bestFitnessText;
    [SerializeField] Text numGenerationsText;
    [SerializeField] Transform populationTextParent;
    [SerializeField] Text textPrefab;

    private GeneticAlgorithm<char> ga;
    private System.Random random;
    private string targetString;

    private string fullPath;

    void Start()
    {
        targetString = TargetString(targetStringSize);
        targetText.text = targetString;

        if (string.IsNullOrEmpty(targetString))
        {
            Debug.LogError("Target string is null or empty");
            this.enabled = false;
        }

        random = new System.Random();
        ga = new GeneticAlgorithm<char>(populationSize, targetString.Length, random, GetRandomCharacter, FitnessFunction, elitism, selectionType, crossoverType, crossoverRate, mutationRate);

        fullPath = Application.persistentDataPath + "/" + "GeneticSave";
        ga.LoadGeneration(fullPath);
    }

    void Update()
    {
        ga.NewGeneration();

        UpdateText(ga.BestGenes, ga.BestFitness, ga.Generation, ga.Population.Count, (j) => ga.Population[j].Genes);

        if (ga.Generation % generationToSave == 0)
        {
            //ga.SaveGeneration(fullPath);
            //this.enable = false;
        }

        if (ga.BestFitness == 1)
        {
            this.enabled = false;
        }

    }

    private char GetRandomCharacter(int geneIndex)
    {
        int i = random.Next(validCharacters.Length);
        return validCharacters[i];
    }

    private float FitnessFunction(int index)
    {
        float score = 0;
        DNA<char> dna = ga.Population[index];

        for (int i = 0; i < dna.Genes.Length; i++)
        {
            if (dna.Genes[i] == targetString[i])
            {
                score += 1;
            }
        }

        score /= targetString.Length;

        score = (Mathf.Pow(5, score) - 1) / (5 - 1);

        return score;
    }

    public string TargetString(int size)
    {
        string targetString = "";
        for (int i = 0; i < size; i++)
        {
            targetString += "1";
        }
        return targetString;
    }



    private int numCharsPerTextObj;
    private List<Text> textList = new List<Text>();

    void Awake()
    {
        numCharsPerTextObj = numCharsPerText / validCharacters.Length;
        if (numCharsPerTextObj > populationSize) numCharsPerTextObj = populationSize;

        int numTextObjects = Mathf.CeilToInt((float)populationSize / numCharsPerTextObj);

        for (int i = 0; i < numTextObjects; i++)
        {
            textList.Add(Instantiate(textPrefab, populationTextParent));
        }
    }

    private void UpdateText(char[] bestGenes, float bestFitness, int generation, int populationSize, Func<int, char[]> getGenes)
    {
        bestText.text = CharArrayToString(bestGenes);
        bestFitnessText.text = bestFitness.ToString();

        numGenerationsText.text = generation.ToString();
        int limit = 20000;
        int counter = 0;
        for (int i = 0; i < textList.Count; i++)
        {
            var sb = new StringBuilder();
            int endIndex = i == textList.Count - 1 ? populationSize : (i + 1) * numCharsPerTextObj;
            for (int j = i * numCharsPerTextObj; j < endIndex; j++)
            {
                foreach (var c in getGenes(j))
                {
                    sb.Append(c);
                }
                counter += sb.ToString().Length;
                if (counter > limit) break;
                if (j < endIndex - 1) sb.AppendLine();
            }
            textList[i].text = sb.ToString();
            if (counter > limit) break;
        }
    }

    private string CharArrayToString(char[] charArray)
    {
        var sb = new StringBuilder();
        foreach (var c in charArray)
        {
            sb.Append(c);
        }

        return sb.ToString();
    }
}
