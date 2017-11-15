using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.XOGameEngine
{
    public enum Player
    {
        O,
        X,
        DRAW
    }

    public sealed class XOEngine
    {//     |    O    | |    X    | |T A B L A|  
     //0000 0000 0000 0 000 0000 00 00 0000 000 0 
     //|-flag 1                                 |-bit za parnost poteza
     // | - flag 2
     //  | - flag 3
     // flag 1 - se odnosi na to da li je igra pokrenuta

        private uint brain = 0x00;
        private static readonly XOEngine instance = new XOEngine();

        #region DOGADJAJI

        /// <summary>
        /// Dogadjaj ce se podici onog trenutka kada odigrani potez bude ispravan!
        /// </summary>
        public event EventHandler<ValidMoveEventArgs> OnValidMove;

        /// <summary>
        /// Dogadjaj ce se podici onog trenutka kada odigrani potez nije ispravan!
        /// </summary>
        public event EventHandler<BadMoveEventArgs> OnBadMove;

        /// <summary>
        /// Dogadjaj ce se podici kada dodje do pobjede u partiji ili nerjesenog rezultata.
        /// Parametar "pobjednik" ce imati vrijednost 1 ako je pobijedio igrac koji je igrao
        /// prvi na potezu,2 ukoliko je pobijedio igrac koji je igrao drugi na potezu
        /// ili 2 ako je bilo nerjeseno.
        /// </summary>
        public event EventHandler<VictoryEventArgs> OnVictory;

        /// <summary>
        /// Dogadjaj ce se podici kada igra nije pokrenuta.
        /// Igra nije pokrenuta dok se ne pozovoe metoda "NewGame"
        /// </summary>
        public event EventHandler<GameStoppedEventArgs> OnGameStopped;

        #endregion

        #region CTOR's

        static XOEngine() // za inicijalizaciju statičkih polja, u suštini ne treba, ali zbog Singleton-a
        {
        }

        private XOEngine() // da ne moze niko instancirati ovaj objekat
        {

        }

        /// <summary>
        /// Readonly prop za dobijanje reference na XO igru
        /// </summary>
        public static XOEngine Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        /// <summary>
        /// Vraca trenutno stanje igre.
        /// </summary>
        public uint GameState
        {
            get { return brain; }
        }

        #region PUBLIC_METHODS

        /// <summary>
        /// Metoda koja na zadatoj poziciji odigrava potez ako je dozvoljen. Poziva akciju transformAction nad objektom 
        /// koji je prosledjen (invokerToTransform).
        /// </summary>
        /// <param name="position">Potez na koji je korisnik odigrao</param>
        /// <param name="transformAction">Akcija(transformacija) koja ce se izvrsiti nad parametrom invokerToTransform</param>
        /// <param name="invokerToTransform">Objekat koji ce biti prosledjen akciji</param>
        public void MakeAMove(int position, Action<object, Player> transformAction, object invokerToTransform)
        {
            if (GameRunning())//Ako nije bilo pobjede
            {
                if (IsPositionEmpty(position))//Ako je potez moguć
                {
                    brain |= (1u << position);//Setuj tu poziciju u mozgu

                    brain ^= 1u;//prvi bit za parnost poteza(invertuj bit partnosti)
                    RecordValidMove(position);//Pozovi metodu za evidentiranje koji je igrac odigrao potez i gdje

                    //javi svima koji slusaju da je odigrano
                    OnValidMove?.Invoke(this, new ValidMoveEventArgs(position));

                    //Pozovi delegat za obradu poteza
                    transformAction(invokerToTransform, (Player)(brain & 1ul));

                    //Provjera da li imamo pobjednika, moglo se spakovati u ona tri bita neki brojac, da se ne
                    //poziva ova metoda prije stvarne mogucnosti da je došlo do pobjede
                    Win();
                }
                else//U slucaju da je odigran potez na zauzeto polje
                {//javimo svima koji slusaju da to nece da moze
                    OnBadMove?.Invoke(this, new BadMoveEventArgs(position));
                }
            }
            else
            {//ako je neko pokusao da igra, a igra nije pokrenuta
                OnGameStopped?.Invoke(this, new GameStoppedEventArgs(true));
            }
        }

        /// <summary>
        /// Metoda koja pokrece novu igru.
        /// </summary>
        /// <param name="switchPlayerTurn">True ako se mijenja poredak igranja</param>
        public void NewGame(bool switchPlayerTurn = false)
        {
            if (switchPlayerTurn)//igrao prvi X, pa onda sl partiju ide O
            {
                brain |= (1u << 30);//Aktiviramo promjenu strana
            }
            if (!GameRunning())
            {
                brain |= (1u << 31);
            }
        }

        /// <summary>
        /// Metoda koja ispituje da li je igra u toku(da li je engine pokrenut).
        /// </summary>
        /// <returns>True ako je igra pokrenuta, false ako nije igra pokrenuta.</returns>
        public bool GameRunning()
        {
            return (brain & 1u << 31) == 1u << 31;
        }

        /// <summary>
        /// Metoda koja stopira(zaustavlja) igru.
        /// </summary>
        public void StopGame()
        {
            // mozak ^= (1u << 31);//Zaustavimo igru, naravno ovo je jednostavnije nego brain=0;
            if (IsSwitchPlayersActive())//Ako je ukljucena zamjena strana
            {
                if ((brain & (1u << 29)) == 0)//Ako je ovde nula, sad igra O prvi
                {
                    brain = 1;
                    brain |= (1u << 29);
                    brain |= (1u << 30);
                }
                else//Onda igra X, dakako
                {
                    brain = 0;
                    brain |= (1u << 30);
                }
            }
            else
            {
                brain = 0;//Zaustavimo igru i ocistimo sva stanja.
            }
        }

        /// <summary>
        /// Metoda za ucitavanje prethodnog stanja igre.
        /// Igra ce biti nastavljena od one tacke u kojoj je igra bila prekinuta.
        /// Pogledati metodu ZauzetePozicijeOdIgraca
        /// </summary>
        /// <param name="state">Stanje igre prilikom prekida.</param>
        public void LoadGame(uint state)
        {
            brain = state;
        }

        /// <summary>
        /// Metoda koja vraca pozicije na koje je prosledjeni igrac odigrao.
        /// </summary>
        /// <param name="p">Igrac (X ili O) za koga zelimo da dobijemo zauzete pozicije.</param>
        /// <returns>Vraca sve pozicije na kojima je upisan prosledjeni igrac p.</returns>
        public IEnumerable<int> ZauzetePozicijeOdIgraca(Player p)
        {
            uint tmp = 0;
            switch (p)
            {
                case Player.O:
                    tmp = (brain >> 18);
                    break;
                case Player.X:
                    tmp = (brain >> 9);
                    break;
                default:
                    break;
            }
            for (int i = 1; i < 10; i++)
            {
                if ((tmp & 1u << i) != 0)
                    yield return i;
            }
        }

        #endregion

        #region PRIVATE_METHODS

        /// <summary>
        /// Metoda koja odredjuje da li je doslo do pobjede i podize odgovarajuci dogadjaj.
        /// Moglo je neko logičnije ime, al' eto.
        /// </summary>
        private void Win()
        {
            #region PRVI_NACIN
            /*
             Mogao sam ovo uraditi na više načina, recimo sa for-om pa da šiftam u jednu stranu.
                Sa pomoćnom, nekako ovako
                     ushort tmp = (ushort)(((mozak & (0x1FF << 10 ili 19)) >> 10 ili 19));
                     ali ovo zahtjeva i šiftanje konstanti.

            Fazon da ne koristim konstante
             ushort tmp;
             if (IsEvenMove())//Ako je paran to je O, ako je neparan to je X
                 tmp = (ushort)(((mozak & (0x1FF << 19)) >> 19));
             else
                 tmp = (ushort)(((mozak & (0x1FF << 10)) >> 10));
           

            U osnovi kako to izgleda
                           987 654 321
             Red {1,2,3} = 000 000 111 => 0 0000 0111 => 0x7   (DEC 7)  << 3
             Red {4,5,6} = 000 111 000 => 0 0011 1000 => 0x38  (DEC 56)
             Red {7,8,9} = 111 000 000 => 1 1100 0000 => 0x1C0 (DEC 448)

             Kol {1,4,7} = 001 001 001 => 0 0100 1001 => 0x49  (DEC 73) << 1
             Kol {2,5,8} = 010 010 010 => 0 1001 0010 => 0x92  (DEC 146)
             Kol {3,6,9} = 100 100 100 => 1 0010 0100 => 0x124 (DEC 292)

             Dia {1,5,6} = 100 010 001 => 1 0001 0001 => 0x111 (DEC 273)
             Dia {3,5,7} = 001 010 100 => 0 0101 0100 => 0x54  (DEC 84)

            za x 10
            za 0 19

            *R:x
            7168
            57344
            458752
            K: X
            74752
            149504
            299008
            Za ****** O ************
            R:O
            3670016
            29360128
            234881024
            k: O
            38273024
            76546048
            153092096
            */
            #endregion

            #region DRUGI_NACIN
            //Kod ovog načina, ne treba mi pomoćna promjenjljiva, ali imaju ova dva velika if-a koja mi se ne sviđaju
            //za X
            /*
            if ((brain & 7168) == 7168 || (brain & 57344) == 57344 || (brain & 458752) == 458752 ||
                (brain & 74752) == 74752 || (brain & 149504) == 149504 || (brain & 299008) == 299008 ||
                (brain & 86016) == 86016 || (brain & 279552) == 279552)
            {
                OnVictory?.Invoke(this, new VictoryEventArgs(Player.X));
                ZaustaviIgru();
            }
            else //za O
            if ((brain & 3670016) == 3670016 || (brain & 29360128) == 29360128 || (brain & 234881024) == 234881024 ||
                (brain & 38273024) == 38273024 || (brain & 76546048) == 76546048 || (brain & 153092096) == 153092096 ||
                (brain & 44040192) == 44040192 || (brain & 143130624) == 143130624)
            {
                OnVictory?.Invoke(this, new VictoryEventArgs(Player.O));
                ZaustaviIgru();
            }
            else
            if ((brain & 0x3FE) == 0x3FE)//Nereseno
            {
                OnVictory?.Invoke(this, new VictoryEventArgs(Player.DRAW));
                ZaustaviIgru();
            }
            */
            #endregion

            /*
             * Konstante za redove, kolone i dijagonalu, pa sad da ne bih isto izračunavao i za
             * OKS, kad to imam već za X, lakše mi je da OKS dovedem na poziciju X-a i da ispitam
             * sve ovo za njega kao za X sto sam uradio. Mora pomocna :S al' eto.
             */
            uint tmp = brain;
            if (IsEvenMove())//igra O
            {
                tmp = (brain >> 9);
            }

            if ((tmp & 7168) == 7168 || (tmp & 57344) == 57344 || (tmp & 458752) == 458752 ||
                (tmp & 74752) == 74752 || (tmp & 149504) == 149504 || (tmp & 299008) == 299008 ||
                (tmp & 86016) == 86016 || (tmp & 279552) == 279552)
            {
                OnVictory?.Invoke(this, new VictoryEventArgs(Winner()));
                StopGame();
            }

            if ((brain & 0x3FE) == 0x3FE)//Dođe iz X u X :S
            {
                OnVictory?.Invoke(this, new VictoryEventArgs(Player.DRAW));
                StopGame();
            }
        }

        /// <summary>
        /// Metoda koja vraca igraca koji je pobjednik partije.
        /// </summary>
        /// <returns>Igraca koji je pobjednik partije.</returns>
        private Player Winner()
        {
            if (((brain & 0x01) == 1))
            {
                return Player.X;
            }
            else
            {
                return Player.O;
            }
        }

        /// <summary>
        /// Metoda za evidentiranje koji je igrac odigrao na koje polje.
        /// Metoda na osnovu bita parnosti poteza odredjuje ko igra kada.
        /// </summary>
        /// <param name="position">Parametar koji odgovara popunjenoj poziciji - poziciji na koju je igrao igrac.</param>
        private void RecordValidMove(int position)
        {
            if (IsEvenMove())//odigrao O
            {
                //mozak |= (1ul << (64 - position * 2));
                brain |= (1u << (18 + position));
            }
            else//odigrao X
            {
                brain |= (1u << (9 + position));
            }
        }

        /// <summary>
        /// Metoda ispituje da li je odigrani potez paran. Prvi potez koji odigra igrac je neparan,drugi je paran!
        /// Na osnovu toga se zakljucuje koji igrac igra trenutno!
        /// </summary>
        /// <returns>Vraca true ako je potez paran,false ako nije.</returns>
        private bool IsEvenMove()
        {
            return (brain & 1ul) == 0 ? true : false;
        }

        /// <summary>
        /// Metoda ispituje da li je odigrani potez moguc-validan.
        /// Potez je validan ako je mjesto(pozicija) slobodno.
        /// </summary>
        /// <param name="position">Mjesto na koje hoce igrac da odigra.</param>
        /// <returns>Vraca true ako je potez validan,suprotno vraca false.</returns>
        private bool IsPositionEmpty(int position)
        {
            return (brain & 1u << position) == 0 ? true : false;
        }

        /// <summary>
        /// Metoda koja ispituje da li je igrac aktivirao zamjenu poredka igraca
        /// </summary>
        /// <returns>True, ako je ukljucena zamjena poredka</returns>
        private bool IsSwitchPlayersActive()
        {
            return (brain & (1u << 30)) == (1u << 30);
        }

        #endregion

        /// <summary>
        /// Metoda za generisanje svih mogucih poteza igraca koji je na potezu.
        /// </summary>
        /// <returns>IEnumerable mogucih poteza igraca.</returns>
        public IEnumerable<int> NextMove()
        {
            for (int i = 1; i < 10; i++)
            {
                if (IsPositionEmpty(i))
                    yield return i;
            }
        }

        public override string ToString()
        {
            // return mozak.ToString("2");

            var s = Convert.ToString(brain, 2).PadLeft(32, '0');

            s = s.Insert(31, " ");
            s = s.Insert(22, " ");
            s = s.Insert(13, " ");
            s = s.Insert(4, " ");

            return new string(s.ToArray<char>());
            // return Convert.ToString(((long)mozak), 2).PadLeft(32, '0');
        }
    }
}
