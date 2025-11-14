## Table Of Contents

<details>
<summary>Details</summary>

  - [Introduction](#Introduction)
  - [Getting Started](#GettingStarted)
  - [Simple Room Placement](#SimpleRoomPlacement)
  - [BSP](#BSP)
  - [Cellular Automata](#CellularAutomata)
  - [Noise Generator](#NoiseGenerator)

</details>

## Introduction
Gen-Proc-Week-1 est un repo contenant le nécessaire de base afin de  customiser des générations procédurales de base.

## Getting Started
Pour démarrer il faudra mettre un objet avec un ```ProceduralGridGenerator``` dans la scène.  

Ensuite, il vous faudra créer un nouveau script dérivant de la classe ```ProceduralGenerationMethod```.  
<img src="Doc/img2.png?raw=true"/>

Faites votre génération procédurale dans ce dernier et créez un asset de ce script.  
Il vous faudra rentrer votre logique dans cette fonction :  
```
protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
{
    //Définition des variables

    // Check for cancellation
    cancellationToken.ThrowIfCancellationRequested();

    // Logique du code

    // Waiting between each steps to see the result progress .
    await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
}
```
Enfin, références cet asset dans votre ProceduralGridGenerator et vous aurez fini l'installation.
votre scene devrait ressembler a ceci :
<img src="Doc/img1.png?raw=true"/>  

## Simple Room Placement
Ce script genere des salle au fur et a mesur en verifiant qu'elle ne s'overlap pas, si elle s'overlap la salle n'est pas crée et essaye d'en placer une autre 
Warning !! le script peut tourner a l'infini si il essaye de mettre des salles sans jamais trouver la place.

Ensuite il va générer des couloirs pour relier les salles créée.  
<img src="Doc/img3.png?raw=true"/>

## BSP
ce script a le meme objectif que celui d'avant mais en utilisant des nodes pour mieux gerer la place et l'organisation des salles.
Il découpe la map jusqu'à atteindre le nombre de nodes enfants demandé. 
Une fois le nombre de nodes enfants atteint il va s'arrêter et instancier des salles à ces positions.
Ensuite il va générer des couloirs pour relier les salles.
<img src="Doc/img4.png?raw=true"/>

## Cellular Automata
Le cellular automata permet de generer du terrain.
Pour cela on genere du noise et on itere sur toutes les tiles une logique pour soit rester de la terre soit de l'eau.
pour cela on check les 8 tiles autours de celle verifier et en fonction des parametres de depart elle changera en fonction du nombre de cell de terre (je recomande 4 ou 5 et 10 iteration ou "step" pour une generation correcte).
Warning !! il ne faut modifier l'etat des tiles seulement apres avoir fait tous les changement pour cela il faut stocker cela dans une liste en locurence dans le code de bool.
<img src="Doc/img5.png?raw=true"/> 

## Noise Generator  
De nouveau une génération de terrain mais bien plus précise. On passera par la librairie FastNoiseLite afin de générer notre bruit. On le créé, lui assigne plusieurs paramètres et la librairie fait le reste il ne nous reste qu'à instancier nos tiles en fonction du résultat.
dans ce code fait on fait varier la hauteur du monde et on le fait 2 fois pour avoir un aspect de grotte.
<img src="Doc/img5.png?raw=true"/>  
