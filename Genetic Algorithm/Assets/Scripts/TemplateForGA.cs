using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TemplateForGA : MonoBehaviour
{
    [Header("Genetic Algorithm")]
    [SerializeField] float maxFitness = float.PositiveInfinity;
    [SerializeField] int dnaSize = 1;
    [SerializeField] string targetString = "To be, or not to be, that is the question.";
    [SerializeField] string validCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ,.|!#$%&/()=? ";
    [SerializeField] int populationSize = 200;
    [SerializeField] CrossoverTypes crossoverType = CrossoverTypes.Uniform;
    [SerializeField] SelectionTypes selectionType = SelectionTypes.Proportional;
    [SerializeField] [Range(0, 1)] float mutationRate = 0.01f;
    [SerializeField] [Range(0, 1)] float crossoverRate = 1f;
    [SerializeField] float elitism = 5;
    [SerializeField] int generationToSave = 10;

    [Header("Other")]
    [SerializeField] int numCharsPerText = 15000;
    [SerializeField] Text bestText;
    [SerializeField] Text bestFitnessText;
    [SerializeField] Text numGenerationsText;
    [SerializeField] Transform populationTextParent;
    [SerializeField] Text textPrefab;

    private GeneticAlgorithm<float> ga;
    private System.Random random;

    private string fullPath;

    void Start()
    {

        random = new System.Random();
        ga = new GeneticAlgorithm<float>(populationSize, dnaSize, random, GetRandomGene, FitnessFunction, elitism, selectionType, crossoverType, crossoverRate, mutationRate);

        fullPath = Application.persistentDataPath + "/" + "GeneticSave";
        ga.LoadGeneration(fullPath);
    }

    void Update()
    {
        ga.NewGeneration();

        //Update UI

        if (ga.Generation % generationToSave == 0)
        {
            ga.SaveGeneration(fullPath);
            //this.enable = false;
        }

        if (ga.BestFitness >= maxFitness)
        {
            this.enabled = false;
        }

    }

    private float GetRandomGene(int geneIndex) 
    {
        //Here you need to find out how to generate a random gene
        //Think range of values for each gene
        //Change function type as needed
        return (float)random.NextDouble();
    }

    private float FitnessFunction(int index)
    {
        float score = 0;
        DNA<float> dna = ga.Population[index];
        
        //TODO: Find an appropiate wat to calculate fitness

        score /= maxFitness;

        //Optional
        score = (Mathf.Pow(5, score) - 1) / (5 - 1);

        return score;
    }

}

