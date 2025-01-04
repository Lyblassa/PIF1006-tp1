using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PIF1006_tp1
{
    public class Automate
    {
        public State InitialState { get; private set; }
        public State CurrentState { get; private set; }
        public List<State> States { get; private set; }
        public bool IsValid { get; private set; }

        public Automate(string filePath)
        {
            States = new List<State>();
            LoadFromFile(filePath);
        }

        private void LoadFromFile(string filePath)
        {
            //chargement du fichier automate
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Erreur : Le fichier '{filePath}' est introuvable.");

                return;
            }

            try
            {
                // Lire les lignes du fichier
                string[] bon = File.ReadAllLines(filePath);
                int initialStateCount = 0;

                // Affiche les lignes pour vérifier (debug)
                foreach (string li in bon)
                {
                    Console.WriteLine($"{li}");
                }


                string[] lignes = File.ReadAllLines(filePath);

                //prend chaque ligne de l'automate et le separe en bloc de caractère
                foreach (string ligne in lignes)
                {
                    string[] parts = ligne.Split(' ');

                    //si on fait face à un état
                    if (parts[0] == "state")
                    {
                        // Analyse de la ligne `state`
                        string etatName = parts[1];// nom automate
                        bool isFinal = parts[2] == "1"; //l'etat est final
                        bool isInitial = parts[3] == "1"; //l'etat est initial

                        // Vérifie si l'état existe déjà
                        foreach (var st in States)
                        {
                            if (st.Name == etatName) // Compare avec les états existants
                            {
                                Console.WriteLine($"Erreur : L'état '{etatName}' existe déjà.");
                                IsValid = false;
                                return;
                            }
                        }

                        // créer enfin l'automate
                        State state = new State(etatName, isFinal);
                        //l'ajouter à la liste des états
                        States.Add(state);

                        if (isInitial)//si initial est 1
                        {
                            InitialState = state;
                            initialStateCount++;
                        }

                    }

                    //si on fait face à une transition
                    else if (parts[0] == "transition")
                    {
                        // Analyse de la ligne `transition`
                        string sourceStateName = parts[1];
                        char input = parts[2][0];//propriete transition
                        string destinationStateName = parts[3];//propriete transition

                        // Recherche des états 
                        State fromState = null;
                        State toState = null;

                        foreach (var state in States)//on se branche a letat initial
                        {
                            if (state.Name == sourceStateName) fromState = state; //etat initial=source
                            if (state.Name == destinationStateName) toState = state;
                        }

                        // on ajoute la transition à la liste des transitions
                        if (fromState != null && toState != null)
                        {
                            fromState.Transitions.Add(new Transition(input, toState));
                        }

                        //on ignore une transition si cette transition comporte un état qui ne figure pas dans la liste des états 
                        else
                        {
                            Console.WriteLine($"Transition ignorée : États '{sourceStateName}' ou '{destinationStateName}' introuvables.");
                        }
                    }
                }

              
                // Vérification si l'automate est déterministe
                foreach (var state in States)
                {
                    var inputs = state.Transitions.GroupBy(t => t.Input);
                    foreach (var group in inputs)
                    {
                        if (group.Count() > 1)
                        {
                            Console.WriteLine($"Erreur : L'automate est non-déterministe à l'état {state.Name} pour l'entrée '{group.Key}'.");
                            IsValid = false;
                            return;
                        }
                    }
                }        


                // si l'automate ne contient aucun état initial, alors il es invalide et on signale une erreur 
                if (States.Count == 0 || InitialState == null)
                {
                    Console.WriteLine("Erreur : Aucun état ou état initial introuvable.");
                    IsValid = false;

                }

                //si l'automate a plus d'un état initial, c'est invalide et on signale l'erreur
                else if (initialStateCount != 1)
                {
                    Console.WriteLine("Erreur : Il doit y avoir exactement un seul état initial.");
                    IsValid = false;

                }

                else
                {
                    // Vérifie s'il y a au moins un état final
                    bool hasFinalState = false;
                    foreach (var state in States)
                    {
                        if (state.IsFinal)
                        {
                            hasFinalState = true;
                            break; // Pas besoin de continuer si on trouve un état final 
                        }
                    }

                    if (!hasFinalState) // Si aucun état final n'est trouvé
                    {
                        Console.WriteLine("Erreur : Aucun état final n'a été défini dans l'automate.");
                        IsValid = false;
                    }

                    //une fois toutes les conditions passées, l'automate est enfin valide!
                    else
                    {
                        IsValid = true; // Marque comme valide si tout se passe bien
                    }
                }
            }

            //si on arrive même pas à lire le fichier 
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la lecture du fichier : {ex.Message}");
                IsValid = false;
            }
            
        }

        //Méthode pour valider une chaîne de caractères entrées par l'utilisateur 
        public bool Validate(string input)
        {
            bool isValid = true;

            // Réinitialiser l'état actuel à l'état initial
            Reset();

            // Transformer l'entrée en une liste de caractères
            List<char> characters = input.ToList();

            // Parcourir chaque caractère de l'entrée
            foreach (char character in characters)
            {
                Transition foundTransition = null;

                // Parcourir toutes les transitions de l'état courant
                foreach (var transition in CurrentState.Transitions)
                {
                    if (transition.Input == character)
                    {
                        foundTransition = transition;
                        break;
                    }
                }

                // Si aucune transition n'est trouvée, marquer comme invalide
                if (foundTransition == null)
                {
                    Console.WriteLine($"Rejeté : Aucune transition trouvée pour '{character}' depuis l'état {CurrentState.Name}.");
                    isValid = false;
                    break;
                }

                // Afficher l'état actuel, l'entrée lue, et l'état suivant
                Console.WriteLine($"État actuel : {CurrentState.Name}, Entrée lue : {character}, Prochain état : {foundTransition.TransiteTo.Name}");

                // Mettre à jour l'état courant
                CurrentState = foundTransition.TransiteTo;
            }

            // Vérifier si l'état courant est final après avoir lu toute la chaîne
            if (isValid && !CurrentState.IsFinal)
            {
                Console.WriteLine($"Rejeté : L'état final atteint {CurrentState.Name} n'est pas un état acceptant.");
                isValid = false;
            }
            else if (isValid)
            {
                Console.WriteLine($"Accepté : L'état final atteint est {CurrentState.Name}.");
            }
            

            return isValid;

        }



        public void Reset()
        {
            // réinitialiser l'automate avant chaque validation.
            CurrentState = InitialState;
        }

        public override string ToString()
        {
            string result = "";
            Console.WriteLine();
            Console.WriteLine("Diagramme d'etat et transition");


            foreach (var state in States)
            {
                // Ajoute l'état avec des crochets ou parenthèses si nécessaire
                if (state == InitialState)
                {
                    result += $"[{state}]\n"; // Appelle le ToString() de la classe State
                }
                else
                {
                    result += $"  {state}\n";
                }

                // Ajoute les transitions de l'état
                foreach (var transition in state.Transitions)
                {
                    result += $"    {transition}\n"; // Appelle le ToString() de la classe Transition
                }

            }

            return result;
        }
    }
}
