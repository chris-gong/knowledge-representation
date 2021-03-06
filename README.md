# A Twist on Social Deduction Games

## Purpose
Our game is based on the well-known “accusation and deception” games such as Town of Salem, Mafia, Trouble in Terrorist Town, and many others in which one player or a group of players out of all the players are assigned the objective to eliminate all the non-murdering players. The difference between our game and the major titles just mentioned is that our game will only involve one player, who is always assigned the role of murderer, and a group of AI, whose job is to determine who the murderer is. We used knowledge representation techniques to map the agent’s knowledge base in the acquisition and filtering of new or existing information. The primary information the agents will use to solve the murder is a series of location and time clues. Using these location and time clues, a graph algorithm will be run on the map of the game to replicate the most likely path each agent (including the player) went on around the time of the murder. We believe that these techniques can be further extended to security systems where incidents happen with many people around. For example, if there is a robbery in a museum with multiple bystanders, we can use location and time clues from cameras in each room of the museum to replicate the path of a supposed robber and match that with one of the civilians present at the time. Note that the path of a robber would look different than that of a murderer and would have to be adjusted accordingly. 

## Rules of the Game
The rules go as follows:

1) The game starts off with you, the player, and 4 agents in 60 second rounds.

2) The player’s movement can be controlled with the click of the mouse. The player’s view can be controlled using the mouse’s scroll wheel and the arrow keys. If the camera needs to be refocused on the center of the player, one can do so by pressing M.

3) In each round, you have to pick up a knife by going up to one and press E to equip it. Then, you must press I and press the Knife button to equip it. Then you can press SpaceBar at any time to kill a nearby agent.
4) At the end of each round, you only lose either if:

    i) You failed to kill an agent, or
  
    ii) You were discovered by the remaining agents as the murderer
  
5) The player wins the game officially once he/she wins the final round that starts off with only the player and 2 agents.

## Game Mechanics
Our game focused primarily on two major game mechanics: knowledge representation and behavior trees. The knowledge representation system consisted of an observable system, a clue-based representation system, and a solving algorithm. The observable system consisted of every agent, including the player, dropping an observable object that represents a fact/atom of where they were at what time. This observable was created fives times a second. Each agent, not including the player, had a knowledge base configured to pick up observables in their field of view. The result was that these facts were converted to Clue objects that were stored in the agent’s knowledge base. As the agent acquired more clues, its knowledge base grew. Lastly, the solving algorithm consisted of retrieving the clues at the times right before and right after the time of the murder. Then, using the location at these clues, a breadth-first search algorithm was run to create the most likely path between the locations and a score was assigned based on the distance of the path and how long the path took. Each agent, excluding the player, ran this algorithm for every other agent, including the player.

The behavior tree component was essential for not only directing the movement of the agents but also controlling how they communicated information. The movement of the agent was also controlled somewhat by the NavMeshAgent component provided by Unity. A NavMesh was generated for the map, and when the agent wanted to travel to the new destination, the NavMeshAgent component would use A* to find a path to the new destination. Since our tree consisted of a sequence of a wandering action and a communication selector, the agent would either choose to wander or communicate with another agent. If it chose to communicate, the agent would follow the agent it wants to exchange information with. Once the agent finally comes into contact with the desired agent, the agent will break out of a loop in the selector, thereby breaking out of the selector. Currently the agent will exchange facts with another agent about candidates they know the least about it at the time. These new facts will be incorporated when solving for the murderer.

