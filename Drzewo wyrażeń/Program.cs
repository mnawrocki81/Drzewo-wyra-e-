using System;
using System.Collections.Generic;

namespace Wyrażenia
{
    interface IObliczalny
    {
        double Oblicz();
    }
    interface ISymbol
    {
        bool CzyOperand();
        bool CzyFunkcja();
    }

    abstract class Wyrażenie : IObliczalny, ISymbol
    {
        public abstract double Oblicz();
        public abstract bool CzyOperand();
        public abstract bool CzyFunkcja();
    }
    abstract class Funkcja : Wyrażenie
    {
        public readonly string nazwa;

        protected Funkcja(string naz)
        { nazwa = naz ?? throw new ArgumentNullException("pusta nazwa funkcji"); }
        public override bool CzyOperand()
        { return false; }
    }
    abstract class Fun0 : Funkcja
    {
        protected Fun0(string naz) : base(naz)
        { }
        public override bool CzyFunkcja()
        { return true; }
    }
    abstract class Fun1 : Fun0
    {
        protected Wyrażenie arg1;

        protected Fun1(string naz, Wyrażenie w1) : base(naz)
        { arg1 = w1 ?? throw new ArgumentNullException("referencja do pustego wyrażenia"); }
        public override string ToString()
        { return nazwa + " (" + arg1 + ")"; }
    }
    abstract class Fun2 : Fun1
    {
        protected Wyrażenie arg2;

        protected Fun2(string naz, Wyrażenie w1, Wyrażenie w2) : base(naz, w1)
        { arg1 = w2 ?? throw new ArgumentNullException("referencja do pustego wyrażenia"); }
        public override string ToString()
        { return nazwa + " (" + arg1 + ", " + arg2 + ")"; }
    }
    abstract class Operand : Wyrażenie
    {
        public override bool CzyOperand()
        { return true; }
        public override bool CzyFunkcja()
        { return false; }
    }
    abstract class Operator : Funkcja
    {
        // priorytety są ujemne dla operatorów 2-argumentowych
        // a dla operatorów 1-argumentowych i funkcji są najwyższe i wynoszą zero
        public readonly int priorytet;

        protected Operator(string naz, int pr) : base(naz)
        { priorytet = pr; }
        public override bool CzyFunkcja()
        { return false; }
        // liczba argumentów
        public abstract int Arność { get; }
    }
    abstract class Operator1 : Operator
    {
        protected Wyrażenie arg1;
  

        protected Operator1(string naz, Wyrażenie w1) : base(naz, 0)
        { arg1 = w1 ?? throw new ArgumentNullException("referencja do pustego wyrażenia"); }
        public override int Arność
        { get { return 1; } }
        public override string ToString()
        {
            if (arg1.CzyOperand() || arg1.CzyFunkcja() || ((Operator)arg1).Arność == 1)
                return nazwa + " " + arg1;
            else return nazwa + " (" + arg1 + ")";
        }
    }
    abstract class Operator2 : Operator1
    {
        protected Wyrażenie arg2;

        public Operator2(string naz, int pr, Wyrażenie w1, Wyrażenie w2) : base(naz, w1)
        { arg2 = w2 ?? throw new ArgumentNullException("referencja do pustego wyrażenia"); }
        public sealed override int Arność
        { get { return 2; } }
        public override string ToString()
        {
            string s = " " + nazwa + " ";
            if (arg1.CzyOperand() || arg1.CzyFunkcja() || ((Operator)arg1).priorytet > priorytet)
                s = arg1 + s;
            else s = "(" + arg1 + ")" + s;
            if (arg2.CzyOperand() || arg2.CzyFunkcja() || ((Operator)arg2).priorytet > priorytet)
                s = s + arg2;
            else s = s + "(" + arg2 + ")";
            return s;
        }
    }

    sealed class Liczba : Operand
    {
        private double liczba;

        public Liczba(double licz)
        { liczba = licz; }
        public override double Oblicz()
        { return liczba; }
        public override string ToString()
        { return Convert.ToString(liczba); }
    }
    sealed class Stała : Operand
    {
        private static Dictionary<string, double> kolekcja =
            new Dictionary<string, double>();
        static Stała()
        {
            kolekcja.Add("pi", Math.PI);
            kolekcja.Add("e", Math.E);
            kolekcja.Add("fi", 1.6180339887498948482);
        }
        public static void DodajStałą(string naz, double wart)
        { kolekcja.Add(naz, wart); }

        private string nazwa;

        public Stała(string naz)
        {
            nazwa = naz ?? throw new ArgumentNullException("brak nazwy stałej");
            if (nazwa.Length == 0) throw new ArgumentException("pusta nazwa stałej");
        }
        public override double Oblicz()
        {
            if (!kolekcja.ContainsKey(nazwa))
                throw new ArgumentOutOfRangeException("nieznana stała");
            double d;
            kolekcja.TryGetValue(nazwa, out d);
            return d;
        }
        public override string ToString()
        { return nazwa; }
    }
    sealed class Zmienna : Operand
    {
        private static Dictionary<string, Double> kolekcja =
            new Dictionary<string, Double>();
        public static void Nowa(string naz)
        { kolekcja.Add(naz, 0); }
        public static void Nowa(string naz, double wart)
        { kolekcja.Add(naz, wart); }
        public static void Ustaw(string naz, double wart)
        { kolekcja.Remove(naz); kolekcja.Add(naz, wart); }
        public static bool Istnieje(string naz)
        { return kolekcja.ContainsKey(naz); }
        public static double Czytaj(string naz)
        { double d; kolekcja.TryGetValue(naz, out d); return d; }
        public static void Usuń()
        { kolekcja.Clear(); }
        public static void Usuń(string naz)
        { kolekcja.Remove(naz); }

        private string nazwa;

        public Zmienna(string naz)
        {
            nazwa = naz ?? throw new ArgumentNullException("brak nazwy zmiennej");
            if (nazwa.Length == 0) throw new ArgumentException("pusta nazwa zmiennej");
        }
        public override double Oblicz()
        {
            if (!kolekcja.ContainsKey(nazwa))
                throw new ArgumentOutOfRangeException("nieznana zmienna");
            double d;
            kolekcja.TryGetValue(nazwa, out d);
            return d;
        }
        public override string ToString()
        { return nazwa; }
    }

    class Przeciwny : Operator1
    {
        public Przeciwny(Wyrażenie w1) : base("-", w1)
        { }
        public override double Oblicz()
        { return -arg1.Oblicz(); }
    }
    class Dodaj : Operator2
    {
        public Dodaj(Wyrażenie w1, Wyrażenie w2) : base("+", -30, w1, w2)
        { }
        public override double Oblicz()
        { return arg1.Oblicz() + arg2.Oblicz(); }
    }
    class Mnóż : Operator2
    {
        public Mnóż(Wyrażenie w1, Wyrażenie w2) : base("*", -20, w1, w2)
        { }
        public override double Oblicz()
        { return arg1.Oblicz() * arg2.Oblicz(); }
    }
    class Potęga : Operator2
    {
        public Potęga(Wyrażenie w1, Wyrażenie w2) : base("^", -10, w1, w2)
        { }
        public override double Oblicz()
        {
            double a = arg1.Oblicz();
            if (a < 0) throw new InvalidOperationException("nie można potęgować ujemnych wartości");
            if (a == 1) return 1;
            double b = arg2.Oblicz();
            if (a == 0)
                if (b <= 0) throw new InvalidOperationException("nie można podnosić 0 do potęgi niedodatniej");
                else return 0;
            if (b == 0) return 1;
            return Math.Pow(a, b);
        }
    }
    class Odejmij : Operator2
    {
        public Odejmij(Wyrażenie w1, Wyrażenie w2) : base("-", -40, w1, w2)
        { }
        public override double Oblicz()
        { return arg1.Oblicz() - arg2.Oblicz(); }
    }
    class Dziel : Operator2
    {
        public Dziel(Wyrażenie w1, Wyrażenie w2) : base("/", -50, w1, w2)
        { }
        public override double Oblicz()
        {
            double b = arg2.Oblicz();
            if (b == 0) throw new InvalidOperationException("nie można dzielić przez 0");
            return arg1.Oblicz() / arg2.Oblicz(); }
    }
    class Modulo : Operator2
    {
        public Modulo(Wyrażenie w1, Wyrażenie w2) : base("%", -50, w1, w2)
        { }
        public override double Oblicz()
        {
            double a = arg1.Oblicz();
            double b = arg2.Oblicz();
            //double mod = a - (a / b) * b;
            if (b == 0) throw new InvalidOperationException("nie można dzielić przez 0");
            return arg1.Oblicz() % arg2.Oblicz();
        }

     }

    class Rnd : Fun0
    {
        private static Random rnd = new Random();

        public Rnd() : base("random")
        { }
        public override double Oblicz()
        { return rnd.NextDouble(); }
        public override string ToString()
        { return nazwa + "()"; }
    }
    sealed class Abs : Fun1
    {
        public Abs(Wyrażenie w1) : base("abs", w1)
        { }
        public override double Oblicz()
        { return Math.Abs(arg1.Oblicz()); }
        public override string ToString()
        { return "|" + arg1 + "|"; }
    }
    sealed class Logarytm : Fun2
    {
        public Logarytm(Wyrażenie w1, Wyrażenie w2) : base("log", w1, w2)
        { }
        public override double Oblicz()
        {
            double a = arg1.Oblicz();
            if (a < 0) throw new InvalidOperationException("nie można logarytmować z ujemną podstawą");
            if (a == 1) throw new InvalidOperationException("nie można logarytmować podstawą 1");
            double b = arg2.Oblicz();
            if (b <= 0) throw new InvalidOperationException("nie można wyciągnąć logatytmu z wartości niedodatniej");
            return Math.Log(b, a);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Zmienna.Nowa("r", 10);
            Zmienna.Nowa("s", 2);
            Zmienna.Ustaw("s", 5); // zmiana wartości z 2 na 5
            Stała.DodajStałą("Euler", 0.5772156649); //dodanie nowej stałej

            Wyrażenie PoleKoła = new Mnóż(new Stała("pi"),
                new Potęga(new Zmienna("r"), new Liczba(2)));
                

            Wyrażenie Dodawanie = new Dodaj(new Zmienna("r"), new Zmienna("s"));
            Wyrażenie Odejmowanie = new Odejmij(new Zmienna("r"), new Zmienna("s"));
            Wyrażenie Mnożenie = new Mnóż (new Zmienna("r"), new Zmienna("s"));
            Wyrażenie Dzielenie = new Dziel (new Zmienna("r"), new Zmienna("s"));
            Wyrażenie Potęgowanie = new Potęga(new Zmienna("r"), new Zmienna("s"));
            Wyrażenie ResztaModulo = new Modulo (new Zmienna("r"), new Zmienna("s"));
            Wyrażenie Przeciwność = new Przeciwny(new Zmienna("r"));
            Wyrażenie NowaStała = new Dodaj(new Liczba(1),new Stała ("Euler"));
            Wyrażenie WartośćBezwzględna = new Abs(new Zmienna("r"));

            Console.Out.WriteLine("{0} = {1}", PoleKoła.ToString(), PoleKoła.Oblicz());
            Console.Out.WriteLine("{0} = {1}", Dodawanie.ToString(), Dodawanie.Oblicz());
            Console.Out.WriteLine("{0} = {1}", Odejmowanie.ToString(), Odejmowanie.Oblicz());
            Console.Out.WriteLine("{0} = {1}", Mnożenie.ToString(), Mnożenie.Oblicz());
            Console.Out.WriteLine("{0} = {1}", Dzielenie.ToString(), Dzielenie.Oblicz());
            Console.Out.WriteLine("{0} = {1}", Potęgowanie.ToString(), Potęgowanie.Oblicz());
            Console.Out.WriteLine("{0} = {1}", ResztaModulo.ToString(), ResztaModulo.Oblicz());
            Console.Out.WriteLine("{0} = {1}", Przeciwność.ToString(), Przeciwność.Oblicz());
            Console.Out.WriteLine("{0} = {1}", NowaStała.ToString(), NowaStała.Oblicz());
            Console.Out.WriteLine("{0} = {1}", WartośćBezwzględna.ToString(), WartośćBezwzględna.Oblicz());

            Console.ReadKey();
        }
    }
}