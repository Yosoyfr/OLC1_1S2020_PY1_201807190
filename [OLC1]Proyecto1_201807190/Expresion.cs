﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _OLC1_Proyecto1_201807190
{
    class Expresion
    {
        //Atributos del objeto
        string nombre;
        List<string> tokens;
        Automata afn;
        Automata afd;

        //Tabla de Transiciones
        List<Tabla_Transiciones> tablaTran;

        public Expresion(string nombre)
        {
            tokens = new List<string>();
            afn = new Automata();
            afd = new Automata();
            this.Nombre = nombre;
            tablaTran = new List<Tabla_Transiciones>();
        }

        public List<string> Tokens { get => tokens; set => tokens = value; }
        public string Nombre { get => nombre; set => nombre = value; }
        public Automata Afn { get => afn; set => afn = value; }
        public Automata Afd { get => afd; set => afd = value; }


        public void convertAFN()
        {
            this.afd.Inicial = this.afn.Inicial;
            Queue cola = new Queue();
            List<Estado> pruebaCerr = new List<Estado>();
            pruebaCerr.Add(this.afn.Inicial);
            List<Estado> cerraduraI = cerradura(pruebaCerr);
            List<List<Estado>> List_Est = new List<List<Estado>>();
            List_Est.Add(cerraduraI);
            cola.Enqueue(cerraduraI);
            foreach (Estado est in this.afn.Estados_Aceptacion)
            {
                if (cerraduraI.Contains(est))
                {
                    this.afd.Estados_Aceptacion.Add(this.afd.Inicial);
                }
            }
            while (cola.Count > 0)
            {
                Estado newInicial = new Estado(varLetra - 65);
                Tabla_Transiciones tabla = new Tabla_Transiciones(newInicial);
                tabla.Cerraduras = List_Est[newInicial.Id];
                List<Estado> aux1 = (List<Estado>)cola.Dequeue();
                foreach (string alf in this.afd.Alfabeto)
                {
                    List<Estado> estados = move(aux1, alf);
                    int auxI = 0;
                    string auxS = "Inicio: " + (char)varLetra + " llega hasta: ";
                    string auxS1 = "";
                    int c = List_Est.Count;
                    List<Estado> newEsts = cerradura(estados);
                    bool perm = true;
                    for (int k = 0; k < c; k++)
                    {
                        if (!compareList(List_Est[k], newEsts))
                        {
                            perm = false;
                            break;
                        }
                        else
                            auxS1 = (char)(66 + auxI) + " simbolo: " + alf;
                        auxI = k;
                    }
                    if (perm)
                    {
                        List_Est.Add(newEsts);
                        cola.Enqueue(newEsts);
                    }
                    //Console.WriteLine(auxS + auxS1);
                    if (auxS1 != "")
                    {
                        Estado newFinal = new Estado(auxI + 1);
                        Transicion tran = new Transicion(newInicial, newFinal, alf);
                        newInicial.Transiciones.Add(tran);
                        tabla.Alcanzados.Add(newFinal);
                        foreach (Estado est in this.afn.Estados_Aceptacion)
                        {
                            if (newEsts.Contains(est))
                            {
                                if (!this.afd.containAceptacion(newFinal))
                                {
                                    this.afd.Estados_Aceptacion.Add(newFinal);
                                }
                            }
                        }
                    }
                    else 
                    {
                        Estado newFinal = new Estado(-1);
                        tabla.Alcanzados.Add(newFinal);
                    }
                }
                tablaTran.Add(tabla);
                this.afd.Estados.Add(newInicial);
                varLetra++;
            }

            this.afd.Tipo = "AFD";
            /*
            foreach (List<Estado> auxList in List_Est)
            {
                string alv = "";
                foreach (Estado aux in auxList)
                {
                    alv += aux.ToString() + ", ";
                }
                Console.WriteLine(alv);
            }
            */
        }

        public bool compareList(List<Estado> list1, List<Estado> list2) 
        {
            List<Estado> result = list2.Except(list1).ToList();
            if (result.Count == 0)
                return false;
            else
                return true;
        }


        public List<Estado> cerradura(List<Estado> estados)
        {
            List<Estado> result = new List<Estado>();
            Stack stackEst = new Stack();
            foreach (Estado est in estados)
            {
                Estado actual = est;
                stackEst.Push(actual);

                while (stackEst.Count > 0)
                {
                    actual = (Estado)stackEst.Pop();
                    foreach (Transicion t in actual.Transiciones)
                    {
                        if (t.Simbolo.Equals("Ɛ") && !result.Contains(t.Final))
                        {
                            result.Add(t.Final);
                            stackEst.Push(t.Final);
                        }
                    }
                }
                result.Add(est);
            }
            /*
            string alv = "";
            foreach (Estado est in result)
            {
                alv += est.Id;
            }
            Console.WriteLine(alv);
            */
            return result;
        }

        int varLetra = 65;
        public List<Estado> move(List<Estado> estados, string simbolo)
        {
            List<Estado> alcanzados = new List<Estado>();
            foreach (Estado est in estados)
            { 
                foreach (Transicion t in est.Transiciones)
                {
                    if (t.Simbolo.Equals(simbolo))
                    {
                        alcanzados.Add(t.Final);
                    }
                }
            }
            /*
            string alv = "";
            foreach (Estado est in alcanzados)
            {
                alv += est.Id;
            }
            Console.WriteLine(alv);
            */
            return alcanzados;
        }

        public void getTabla()
        {
            string auxS = "Estado --- ";
            foreach (string varS in this.afd.Alfabeto)
            {
                auxS += varS + " --- ";
            }
            Console.WriteLine(auxS);
            Console.WriteLine(tablaTran.Count);
            foreach (Tabla_Transiciones auxTran in tablaTran)
            {
                Console.WriteLine(auxTran);
            }
        }

        public string getDOTAFN()
        {
            string graph = " digraph G {\n " +
                "rankdir=LR;\n " +
                "node [shape=doublecircle]; " + afn.getDOTAceptacion() + ";\n" +
                "node[shape = circle];\n 	" +
                "s [style=invis];\n " +
                "s-> 0[label = \"Inicio\"];\n ";

            graph += afn.getDOT();

            graph += "}";

            return graph;
        }

        public string getDOTAFD()
        {
            string graph = " digraph G {\n " +
                "rankdir=LR;\n " +
                "node [shape=doublecircle]; " + afd.getDOTAceptacion() + ";\n" +
                "node[shape = circle];\n 	" +
                "s [style=invis];\n " +
                "s-> 0[label = \"Inicio\"];\n ";

            graph += afd.getDOT();

            graph += "}";

            return graph;
        }

        public string getDOTTabla()
        {
            string graph = 
                " digraph G {\n " +
                "rankdir=LR;\n " +
                "node [shape = record, style = filled, color = white];\n " +
                "tbl [label=<\n";

            graph += "\t<table>\n";
            graph += "\t\t<tr>\n\t\t\t<td>Estados del AFN</td>\n";
            graph += "\t\t<td>Estado del AFD</td>\n";
            foreach (string varS in this.afd.Alfabeto)
            {
                graph += "\t\t\t<td>" + varS + "</td>\n";
            }
            graph += "\t\t</tr>\n";

            foreach (Tabla_Transiciones auxTran in tablaTran)
            {
                graph += "\t\t" + auxTran.getDOT() + "\n";
            }

            graph += "\t</table>\n>];\n}";

            return graph;
        }

        public void evaluacionExpresiones(string lexema) 
        {
            Console.WriteLine(this.nombre);
            Console.WriteLine(lexema);
        }
    }
}
