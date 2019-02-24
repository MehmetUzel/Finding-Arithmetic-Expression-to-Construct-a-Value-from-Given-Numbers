using System;

namespace Arithmetics
{
    class Program
    {

        static int[] ReadInput(out int value)
        {
            Console.Write("Enter integer numbers to use (space-separated): ");
            string s = Console.ReadLine();
            string[] parts = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int[] a = new int[parts.Length];
            for (int i = 0; i < a.Length; i++)
                a[i] = int.Parse(parts[i]);

            Console.Write("Enter integer value to calculate: ");
            value = int.Parse(Console.ReadLine());

            return a;

        }

        static void SolveAndPrint(int[] numbers, int targetValue)
        {

            int targetKey = (targetValue << numbers.Length) + (1 << numbers.Length) - 1;
            // (value << numbers.Length) represents expression value
            // (1 << numbers.Length) - 1 represents mask with all bits set to 1,
            // i.e. mask in which each input number has been used exactly once
            // to build the expression.

            System.Collections.Generic.HashSet<int> solvedKeys =
                new System.Collections.Generic.HashSet<int>();
            // Each number in the collection indicates that corresponding value + mask
            // has been reached using arithmetical operations.

            System.Collections.Generic.Dictionary<int, int> keyToLeftParent =
                new System.Collections.Generic.Dictionary<int, int>();
            // For each solved key (value + mask), there is an entry indicating
            // result of the expression on the left side of the arithmetic
            // operator. Missing value indicates that key represents the
            // raw number (taken from the input list), rather than
            // the result of a calculation.

            System.Collections.Generic.Dictionary<int, int> keyToRightParent =
                new System.Collections.Generic.Dictionary<int, int>();
            // Same as keyToLeftParent, only indicating the right parent
            // used to build the expression.

            System.Collections.Generic.Dictionary<int, char> keyToOperator =
                new System.Collections.Generic.Dictionary<int, char>();
            // Indicates arithmetic operator used to build this node
            // from left and right parent nodes. Missing value for a given key
            // indicates that key is a raw value taken from input array,
            // rather than result of an arithmetic operation.

            System.Collections.Generic.Queue<int> queue =
                new System.Collections.Generic.Queue<int>();
            // Keys (value + mask pairs) that have not been processed yet

            // First step is to initialize the structures:
            // Add all input values into corresponding array entries and
            // add them to the queue so that the operation can begin

            for (int i = 0; i < numbers.Length; i++)
            {

                int key = (numbers[i] << numbers.Length) + (1 << i);

                solvedKeys.Add(key);
                queue.Enqueue(key);

            }

            // Now expand entries one at the time until queue is empty,
            // i.e. until there are no new entries populated.
            // Additional stopping condition is that target key has been generated,
            // which indicates that problem has been solved and there is no need to
            // expand nodes any further.

            while (queue.Count > 0 && !solvedKeys.Contains(targetKey))
            {

                int curKey = queue.Dequeue();

                int curMask = curKey & ((1 << numbers.Length) - 1);
                int curValue = curKey >> numbers.Length;

                // Now first take a snapshot of all keys that
                // have been reached because this collection is going to
                // change during the following operation

                int[] keys = new int[solvedKeys.Count];
                solvedKeys.CopyTo(keys);

                for (int i = 0; i < keys.Length; i++)
                {

                    int mask = keys[i] & ((1 << numbers.Length) - 1);
                    int value = keys[i] >> numbers.Length;

                    if ((mask & curMask) == 0)
                    { // Masks are disjoint, i.e. two entries do not use
                      // the same input number twice.
                      // This is sufficient condition to combine the two entries

                        for (int op = 0; op < 6; op++)
                        {

                            char opSign = '\0';
                            int newValue = 0;

                            switch (op)
                            {
                                case 0: // Addition
                                    newValue = curValue + value;
                                    opSign = '+';
                                    break;
                                case 1: // Subtraction - another value subtracted from current
                                    newValue = curValue - value;
                                    opSign = '-';
                                    break;
                                case 2: // Subtraction - current value subtracted from another
                                    newValue = value - curValue;
                                    opSign = '-';
                                    break;
                                case 3: // Multiplication
                                    newValue = curValue * value;
                                    opSign = '*';
                                    break;
                                case 4: // Division - current divided by another
                                    newValue = -1;  // Indicates failure to divide
                                    if (value != 0 && curValue % value == 0)
                                        newValue = curValue / value;
                                    opSign = '/';
                                    break;
                                case 5: // Division - other value divided by current
                                    newValue = -1;  // Indicates failure to divide
                                    if (curValue != 0 && value % curValue == 0)
                                        newValue = value / curValue;
                                    opSign = '/';
                                    break;
                            }

                            if (newValue >= 0)
                            {   // Ignore negative values - they can always be created
                                // the other way around, by subtracting them
                                // from a larger value so that positive value is reached.

                                int newMask = (curMask | mask);
                                // Combine the masks to indicate that all input numbers
                                // from both operands have been used to produce
                                // the resulting expression

                                int newKey = (newValue << numbers.Length) + newMask;

                                if (!solvedKeys.Contains(newKey))
                                {   // We have reached a new entry.
                                    // This expression should now be added
                                    // to data structures and processed further
                                    // in the following steps.

                                    // Populate entries that describe newly created expression
                                    solvedKeys.Add(newKey);

                                    if (op == 2 || op == 5)
                                    {   // Special cases - antireflexive operations
                                        // with interchanged operands
                                        keyToLeftParent.Add(newKey, keys[i]);
                                        keyToRightParent.Add(newKey, curKey);
                                    }
                                    else
                                    {
                                        keyToLeftParent.Add(newKey, curKey);
                                        keyToRightParent.Add(newKey, keys[i]);
                                    }

                                    keyToOperator.Add(newKey, opSign);

                                    // Add expression to list of reachable expressions
                                    solvedKeys.Add(newKey);

                                    // Add expression to the queue for further expansion
                                    queue.Enqueue(newKey);

                                }

                            }

                        }
                    }
                }

            }

            // Now print the solution if it has been found

            if (!solvedKeys.Contains(targetKey))
                Console.WriteLine("Solution has not been found.");
            else
            {
                PrintExpression(keyToLeftParent, keyToRightParent, keyToOperator, targetKey, numbers.Length);
                Console.WriteLine("={0}", targetValue);
            }
        }

        static void PrintExpression(System.Collections.Generic.Dictionary<int, int> keyToLeftParent,
                                    System.Collections.Generic.Dictionary<int, int> keyToRightParent,
                                    System.Collections.Generic.Dictionary<int, char> keyToOperator,
                                    int key, int numbersCount)
        {

            if (!keyToOperator.ContainsKey(key))
                Console.Write("{0}", key >> numbersCount);
            else
            {

                Console.Write("(");

                // Recursively print the left operand
                PrintExpression(keyToLeftParent, keyToRightParent, keyToOperator,
                                keyToLeftParent[key], numbersCount);

                // Then print the operation sign
                Console.Write(keyToOperator[key]);

                // Finally, print the right operand
                PrintExpression(keyToLeftParent, keyToRightParent, keyToOperator,
                                keyToRightParent[key], numbersCount);

                Console.Write(")");

            }

        }

        static void Main(string[] args)
        {

            while (true)
            {

                int value;
                int[] numbers = ReadInput(out value);

                SolveAndPrint(numbers, value);

                Console.Write("More? (y/n) ");
                if (Console.ReadLine().ToLower() != "y")
                    break;

            }
        }
    }
}