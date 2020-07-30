# Genetic_Algorithm_For_Unity
This is a framework implementation of the genetic algorithm in Unity using generic types. 
This Unity project comes with problem 3 examples: One Max, 3-SAT and sentence finder

The base of this project originates from a [tutorial](https://forum.unity.com/threads/tutorial-genetic-algorithm-c.479062/ "Tutorial-Genetic-Algorithm") in the Unity forum created by a user called Kryzarel. 
I used the assets from this tutorial and build on top to optimize the algorithm and added more features to improve the results of the search. 
Some of the things that I added include new exposed parameters including:
- Crossover types: Uniform, OnePoint, TwoPoint, UniformAverage, UniformWeightedAverage.
- Crossover rate: which determines if crossover will take place for a given pair of individuals
- Selection types: Proportional, Rank, Tournament, Random.
- Mutation rate: which determines how much of an individual will be mutated.
- Elitism: Fixed number of individuals or percentage of the population.
 
