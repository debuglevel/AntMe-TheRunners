using System;
using System.Collections.Generic;
using System.Collections;

using AntMe.Deutsch;


// Füge hier hinter AntMe.Spieler einen Punkt und deinen Namen ohne Leerzeichen
// ein! Zum Beispiel "AntMe.Spieler.WolfgangGallo".
namespace AntMe.Spieler.debuglevel
{

    #region AmeisenDefinitionen

    // Das Spieler-Attribut erlaubt das Festlegen des Volk-Names und von Vor-
    // und Nachname des Spielers. Der Volk-Name muß zugewiesen werden, sonst wird
    // das Volk nicht gefunden.
    [Spieler(
        Volkname = "The Runners",
        Vorname = "debuglevel.de"
        //Nachname = "XXXX"
    )]

    // Das Typ-Attribut erlaubt das Ändern der Ameisen-Eigenschaften. Um den Typ
    // zu aktivieren muß ein Name zugewiesen und dieser Name in der Methode 
    // BestimmeTyp zurückgegeben werden. Das Attribut kann kopiert und mit
    // verschiedenen Namen mehrfach verwendet werden.
    // Eine genauere Beschreibung gibts in Lektion 6 des Ameisen-Tutorials.
    
    [Kaste(
        // Standard-Ameise. Unbenutzt
        Name = "Standard",
        GeschwindigkeitModifikator = 0,
        DrehgeschwindigkeitModifikator = 0,
        LastModifikator = 0,
        ReichweiteModifikator = 0,
        SichtweiteModifikator = 0,
        EnergieModifikator = 0,
        AngriffModifikator = 0
    )]

    [Kaste(
        // ist schnell und kann viel tragen
        Name = "ObstSammler",
        GeschwindigkeitModifikator = 2,
        DrehgeschwindigkeitModifikator = -1,
        LastModifikator = 2,
        ReichweiteModifikator = 0,
        SichtweiteModifikator = -1,
        EnergieModifikator = -1,
        AngriffModifikator = -1
    )]

    [Kaste(
        // ist schnell und kann viel tragen
        Name = "ZuckerSammler",
        GeschwindigkeitModifikator = 2,
        DrehgeschwindigkeitModifikator = -1,
        LastModifikator = 2,
        ReichweiteModifikator = -1,
        SichtweiteModifikator = 0,
        EnergieModifikator = -1,
        AngriffModifikator = -1
    )]

    [Kaste(
        // besonders Schnell, besonders Weitsichtig
        // unbenutzt
        Name = "Späher",
        GeschwindigkeitModifikator = 2,
        DrehgeschwindigkeitModifikator = -1,
        LastModifikator = -1,
        ReichweiteModifikator = 0,
        SichtweiteModifikator = 2,
        EnergieModifikator = -1,
        AngriffModifikator = -1
    )]

    [Kaste(
        // energiereich und Angriffskraft
        // unbenutzt
        Name = "Kämpfer",
        GeschwindigkeitModifikator = 0,
        DrehgeschwindigkeitModifikator = 0,
        LastModifikator = -1,
        ReichweiteModifikator = 0,
        SichtweiteModifikator = -1,
        EnergieModifikator = 1,
        AngriffModifikator = 1
     )]

    [Kaste(
        // unbenutzt
        Name = "Dummy",
        GeschwindigkeitModifikator = -1,
        DrehgeschwindigkeitModifikator = -1,
        LastModifikator = -1,
        ReichweiteModifikator = -1,
        SichtweiteModifikator = -1,
        EnergieModifikator = -1,
        AngriffModifikator = -1
    )]

    #endregion

    public class debuglevelAmeise : Basisameise
    {
        #region zusätzliche Variablen

        // ID der Ameise
        int id = 0;

        // Ziel, welches die Ameise direkt (d.h. über eigene Laufroutine) ansteuert
        Spielobjekt direktziel = null;
        ZielArten direktzielArt = ZielArten.kein;

        // zeigt an, ob die Ameise verhungert, wenn sie nicht sofort zum Bau umkehrt
        bool mussSofortZumBau = false;

        #endregion


        #region Kaste

        /// <summary>
        /// Bestimmt die Kaste einer neuen Ameise.
        /// </summary>
        /// <param name="anzahl">Die Anzahl der von jeder Kaste bereits vorhandenen Ameisen.</param>
        /// <returns>Der Name der Kaste der Ameise.</returns>
        public override string BestimmeKaste(Dictionary<string, int> anzahl)
        {
            id = Brain.NeueAmeisenID();

            // zufällig ZuckerSammler und Obstsammler im Verhältnis 80:20 generieren
            int zufall = Zufall.Zahl(1, 101);

            if ((zufall >= 1) && (zufall <= 80))
            {
                return "ZuckerSammler";
            }
            else
            {
                return "ObstSammler";
            }
        }

        #endregion

        #region Fortbewegung

        /// <summary>
        /// Wird wiederholt aufgerufen, wenn die Ameise nicht weiss wo sie
        /// hingehen soll.
        /// </summary>
        public override void Wartet()
        {
            Debug("Wartet()");

            if ((gibZiel() != null) && ((Brain.Bau != null) && (gibZiel() != Brain.Bau)))
            {
                // wenn die Ameise wartet (außer, wenn sie den Bau als Ziel hat), aber ein Ziel hat,
                // ist wohl was schiefgelaufen (Haufen mittlerweile leer).
                // daher Ziel entfernen.
                setzeDirektziel(null);    
            }

            if (esGibtArbeit())
            {
                Debug("Wartet(): MacheArbeit()");
                macheArbeit();
            }
            else
            {
                Debug("Wartet(): WerdeHilfsSpäher()");
                werdeHilfsSpäher();
            }

        }

        /// <summary>
        /// Wird einmal aufgerufen, wenn die Ameise ein Drittel ihrer maximalen
        /// Reichweite überschritten hat.
        /// </summary>
        public override void WirdMüde()
        {
            // unbenutzt, da von eigener Laufroutine abgedeckt
        }

        #endregion

        #region Nahrung

        /// <summary>
        /// Wird wiederholt aufgerufen, wenn die Ameise mindestens einen
        /// Zuckerhaufen sieht.
        /// </summary>
        /// <param name="zucker">Der nächstgelegene Zuckerhaufen.</param>
        public override void Sieht(Zucker zucker)
        {
            // gesehene Objekte immer merken
            Brain.MerkeObjekt(zucker);

            // wenn Ameise sofort zum Bau muss, dann alles andere ignorieren
            if (mussSofortZumBau)
            {
                return;
            }

            if (Kaste == "ZuckerSammler")
            {
                if ((hatZiel() == ZielArten.kein) || hatZiel() == ZielArten.circa)
                {    
                    // hat kein Ziel oder nur ein ungefähres (dann ist dieser Haufen wohl sein ungefähres Ziel),
                    // dann schlussendlich exakt auf diesen Haufen gehen
                    // d.h. eigene Laufroutine deaktivieren (AntMe erwartet, dass der letzte Schritt der Ameise zum Objekt durch GeheZuZiel() stattfindet)
                    setzeDirektziel(null);
                    GeheZuZiel(zucker); 
                }
            }
        }

        /// <summary>
        /// Wird wiederholt aufgerufen, wenn die Ameise mindstens ein
        /// Obststück sieht.
        /// </summary>
        /// <param name="obst">Das nächstgelegene Obststück.</param>
        public override void Sieht(Obst obst)
        {
            // Objekte immer merken
            Brain.MerkeObjekt(obst, this);  

            if (mussSofortZumBau)
            {
                return;
            }

            if (Kaste == "ObstSammler")
            {

                if ((BrauchtNochTräger(obst)) && ((hatZiel() == ZielArten.kein) || hatZiel() == ZielArten.circa))  
                {
                    // hat kein Ziel oder nur ein ungefähres (dann ist dieser Haufen wohl sein ungefähres Ziel),
                    // dann exakt auf den Haufen losgehen
                    setzeDirektziel(null);
                    GeheZuZiel(obst);

                    Debug("Sieht(): GeheZuZiel(obst)");
                }
            }
        }

        /// <summary>
        /// Wird einmal aufgerufen, wenn die Ameise einen Zuckerhaufen als Ziel
        /// hat und bei diesem ankommt.
        /// </summary>
        /// <param name="zucker">Der Zuckerhaufen.</param>
        public override void ZielErreicht(Zucker zucker)
        {
            if (Kaste == "ZuckerSammler")
            {
                Nimm(zucker);

                // falls der Zuckerhaufen leer ist, dann Objekt vergessen
                if (zucker.Menge == 0)
                {
                    Brain.VergesseObjekt(zucker);
                }

                GeheDirektZuBau();
            }

        }

        /// <summary>
        /// Wird einmal aufgerufen, wenn die Ameise ein Obststück als Ziel hat und
        /// bei diesem ankommt.
        /// </summary>
        /// <param name="obst">Das Obstück.</param>
        public override void ZielErreicht(Obst obst)
        {
            // nur wenn nicht mittlerweile schon andere zur Hilfe gekommen sind.
            if ((Kaste == "ObstSammler") && (BrauchtNochTräger(obst)))  
            {
                // TODO:
                // Überlegung: Ameisen nehmen derzeit Obst sofort auf und gehen zum Bau - auch wenn nicht genug Ameisen da sind (das verwirrt die Laufroutine)
                // Verbessungs-Idee: Ist es besser, zu warten, bis alle Ameisen da sind, und erst dann loszulaufen?
                Debug("ZielErreicht: Nehme Obst auf.");
                Nimm(obst);
                //BleibStehen();  // XXX: Warum bleibt er nicht stehen?
                GeheDirektZuBau();
            }
        }

        #endregion

        #region Kommunikation

        /// <summary>
        /// Wird einmal aufgerufen, wenn die Ameise eine Markierung des selben
        /// Volkes riecht. Einmal gerochene Markierungen werden nicht erneut
        /// gerochen.
        /// </summary>
        /// <param name="markierung">Die nächste neue Markierung.</param>
        public override void RiechtFreund(Markierung markierung)
        {
        }

        /// <summary>
        /// Wird wiederholt aufgerufen, wenn die Ameise mindstens eine Ameise des
        /// selben Volkes sieht.
        /// </summary>
        /// <param name="ameise">Die nächstgelegene befreundete Ameise.</param>
        public override void SiehtFreund(Ameise ameise)
        {
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Ameise eine befreundete Ameise eines anderen Teams trifft.
        /// </summary>
        /// <param name="ameise"></param>
        public override void SiehtVerbündeten(Ameise ameise)
        {
        }

        #endregion

        #region Kampf

        /// <summary>
        /// Wird wiederholt aufgerufen, wenn die Ameise mindestens eine Wanze
        /// sieht.
        /// </summary>
        /// <param name="wanze">Die nächstgelegene Wanze.</param>
        public override void SiehtFeind(Wanze wanze)
        {
            // Objekte immer merken
            Brain.MerkeObjekt(wanze);  

            if (Kaste == "Kämpfer")
            {
                // hat kein Ziel oder nur ein ungefähres (dann ist dieser Haufen wohl sein ungefähres Ziel),
                // dann auf die Wanze losgehen
                if ((wanze.AktuelleEnergie != 0) && (hatZiel() == ZielArten.kein) || hatZiel() == ZielArten.circa)  
                {
                    setzeDirektziel(null);
                    GreifeAn(wanze);
                }
            }
        }

        /// <summary>
        /// Wird wiederholt aufgerufen, wenn die Ameise mindestens eine Ameise eines
        /// anderen Volkes sieht.
        /// </summary>
        /// <param name="ameise">Die nächstgelegen feindliche Ameise.</param>
        public override void SiehtFeind(Ameise ameise)
        {
        }

        /// <summary>
        /// Wird wiederholt aufgerufen, wenn die Ameise von einer Wanze angegriffen
        /// wird.
        /// </summary>
        /// <param name="wanze">Die angreifende Wanze.</param>
        public override void WirdAngegriffen(Wanze wanze)
        {
            // TODO: Hier sollte selbst bei Sammlern etwas rein.
        }

        /// <summary>
        /// Wird wiederholt aufgerufen in der die Ameise von einer Ameise eines
        /// anderen Volkes Ameise angegriffen wird.
        /// </summary>
        /// <param name="ameise">Die angreifende feindliche Ameise.</param>
        public override void WirdAngegriffen(Ameise ameise)
        {
        }

        #endregion

        #region Sonstiges

        /// <summary>
        /// Wird einmal aufgerufen, wenn die Ameise gestorben ist.
        /// </summary>
        /// <param name="todesart">Die Todesart der Ameise</param>
        public override void IstGestorben(Todesart todesart)
        {
            //Debug("Gestorben wegen: " + todesart);
        }

        /// <summary>
        /// Wird unabhängig von äußeren Umständen in jeder Runde aufgerufen.
        /// </summary>
        public override void Tick()
        {
            // Memo: Tick() wird nach (allen?) anderen Funktionen aufgerufen.

            //Debug("Tick()");
            prüfeLaufstrecke(); // Prüfen, ob die Ameise stirbt, wenn sie nicht sofort zum Bau geht
            SetzeBau(); // den Ameisen-Bau als Objekt abspeichern
            prüfeKurzVorBau();  // prüfen, ob die Ameise kurz vor dem Bau ist und die exakte GeheZuBau-Methode greifen soll.
            //PrüfeHilfsSpäherArbeit();   // Falls die Ameise Hilfsspäher ist, wird nach Arbeit gesucht.
        }

        #endregion


        #region zusätzliche Funktionen

        #region Strecken, Entfernungen

        // wenn GeheDirektZuBau() aktiv ist, muss kurz vor dem Bau zum exakten GeheZuBau() übergeben werden.
        private void prüfeKurzVorBau()
        {
            if (Brain.Bau == null)  // wenn der Bau noch nicht bekannt ist, wird GeheDirektZuBau gar nicht verwendet.
            {
                return;
            }

            // wenn das Ziel der Bau ist, und die Ameise kurz davorsteht: übergeben.
            // TODO: Statt < 50 irgendwas kleineres, dynamisches?
            // MEMO: scheint mit 50 eigentlich ganz okay zu sein. Kleinere Werte scheinen keinen Vorteil zu bringen.
            if ((gibZiel() == Brain.Bau) && (Koordinate.BestimmeEntfernung(this, Brain.Bau) < 50))
            {
                setzeDirektziel(null);
                GeheZuBau();
            }
        }

        // Prüft, ob die Ameise stirbt wenn sie nicht sofort zum Bau geht und schickt sie ggf dort hin.
        private void prüfeLaufstrecke()
        {
            if ((mussSofortZumBau) && (übrigeLaufstrecke() < EntfernungZuBau))
            {
                //Debug("Laufstrecke überschritten. Zu doof um richtig zu laufen. Stirbt demnächst. (Übrig: " + ÜbrigeLaufstrecke() + " | Bau: " + EntfernungZuBau + ")");
            }
            
            // wenn die Ameise in der aktuellen Runde es gerade noch zum Bau schafft ohne zu sterben (und nicht bereits angewiesen wurde, zum Bau zu gehen).
            // (TODO: 20 durch einen dynamischen Wert ersetzen)
            if ((übrigeLaufstrecke() - 20 < EntfernungZuBau) && (mussSofortZumBau == false)) 
            {
                mussSofortZumBau = true;
                //Debug("Laufstrecke grenzwertig. Derzeit noch " + ÜbrigeLaufstrecke() + " Schritte. Bau ist " + EntfernungZuBau + " entfernt. Gehe zum Bau.");
                GeheDirektZuBau();
            }
            else if (((übrigeLaufstrecke() - 20 < EntfernungZuBau) == false) && (mussSofortZumBau == true))  
            {
                //wenn sie nicht mehr zum Bau muss, davor aber musste: wieder false einstellen.

                //Debug("MussZumBau zurückgesetzt.");
                mussSofortZumBau = false;
            }
        }

        //gibt die restliche Strecke zurück, die die Ameise noch gehen kann
        private int übrigeLaufstrecke()
        {
            return Reichweite - ZurückgelegteStrecke;
        }

        #endregion

        #region Gehen

        // verbesserte GeheZuZiel-Methode (eigene Laufroutine)
        private void geheDirektZuZiel(Spielobjekt ziel)
        {
            DreheInRichtung(Koordinate.BestimmeRichtung(this, ziel));

            int entfernung = Koordinate.BestimmeEntfernung(this, ziel);
            if (entfernung == 0)    // bei Markierungen z.B. wird der Radius abgezogen... resultiert dann manchmal/oft in 0. Dann einfach mal ewig weit laufen lassen.
            {
                entfernung = 9999;
                setzeDirektziel(ziel, ZielArten.circa);
            }
            else
            {
                setzeDirektziel(ziel, ZielArten.genau);
            }

            GeheGeradeaus(entfernung);
            //Debug("GeheDirektZuZiel()");
        }

        // neue GeheZuBau-Methode. Soll in Verbindung mit besserer GeheZuZiel-Methode schnellere Wege (ohne "Rumwuseln") machen
        public void GeheDirektZuBau()
        {
            if (Brain.Bau != null)  // Brain kennt den Bau - lasse sie direkt dorthin gehen
            {
                geheDirektZuZiel(Brain.Bau);
            }
            else  // Brain kennt den Bau noch nicht - alte Methode verwenden
            {
                GeheZuBau();
            }
        }

        #endregion

        #region Ziele

        public enum ZielArten
        {
            kein = 0,
            genau = 1,
            circa = 2
        }

        // setzt ein Ziel
        private void setzeDirektziel(Spielobjekt spielobjekt)
        {
            direktziel = spielobjekt;
            if (spielobjekt == null)
            {
                setzeZielArt(ZielArten.kein);
            }
        }

        // setze ein Ziel und ZielArt
        private void setzeDirektziel(Spielobjekt spielobjekt, ZielArten zielart)
        {
            setzeDirektziel(spielobjekt);
            setzeZielArt(zielart);
        }

        // setzt eine ZielArt (kein, genau, circa)
        private void setzeZielArt(ZielArten zielart)
        {
            direktzielArt = zielart;
        }

        private ZielArten hatZiel()
        {
            if ((direktziel == null) && (Ziel == null)) // hat kein Ziel
            {
                return ZielArten.kein;
            }
            else if (((direktziel != null) && (direktzielArt == ZielArten.genau)) || (Ziel != null))  // hat ein Ziel - und zwar ein genaues
            {
                return ZielArten.genau;
            }
            else if ((direktziel != null) && (direktzielArt == ZielArten.circa)) // hat ein Ziel - aber nur ungefähr
            {
                return ZielArten.circa;
            }

            Debug("Das sollte nicht passieren. (36583)");
            return 0;
        }

        private Spielobjekt gibZiel()
        {
            if ((direktziel != null) && (Ziel != null)) // beide Ziele sollten eher nicht gleichzeitig gesetzt sein...
            {
                Debug("OMG! Ich habe Ziel UND Direktziel:");
                Debug("Ziel: " + Ziel);
                Debug("DirektZiel: " + direktziel);
            }

            if (Ziel != null)
            {
                return Ziel;
            }
            else if (direktziel != null)
            {
                return direktziel;
            }

            return null;
        }

        #endregion

        #region Sonstiges

        // kleine, doofe Funktion, die dauernd aufgerufen wird, damit man die Referenz vom Bau bekommt.
        public void SetzeBau()
        {
            // In der Regel ist eine Späherameise die erste, die überhaupt wieder zum Bau muss (weil sie erschöpft ist). Daher ist die Bedingung ausreichend.
            if ((Brain.Bau == null) && (mussSofortZumBau))   // Unter dieser Bedingung haben sie das Ziel 'Bau'
            {
                Brain.SetBau(Ziel);
            }

            return;

            /*
             //Alternative Bedingungen, unter denen Ameisen zurückmüssen.
            if ((Brain.Bau==null) && (Ziel!=null) && (Kaste == "ZuckerSammler") && (AktuelleLast!=0)) //Unter diesen Bedingungen geht die Ameise hoffentlich gerade zum Bau. (besch... Workaround)
            {
                Brain.setBau(Ziel);
            }
            if ((Brain.Bau == null) && (GibZiel() != null) && (Kaste == "Späher") && (MussSofortZumBau))    //Unter diesen Umständen geht die SpäherAmeise wohl gerade zum Bau
            {
                Brain.setBau(Ziel);
            }
            */
        }

        #endregion

        #region Hilfsspäher, Arbeit

        // Diese Funktion wird aufgerufen, wenn die Ameise nichts zu tun hätte und nur rumstehen würde.
        // Dreht die Ameise in eine zufällige Richtung und schickt sie auf Erkundungstour
        private void werdeHilfsSpäher()
        {
            DreheUmWinkel(Zufall.Zahl(0, 360));
            GeheGeradeaus();
            Debug("WerdeHilfsSpäher()");
        }

        // Diese Funktion soll HilfsSpäher unterbrechen, sobald es wieder Arbeit gibt.
        private void prüfeHilfsSpäherArbeit()
        {
            if (gibZiel() == null)
            { 
                //unter dieser Bedingung ist die Ameise (glaube ich) arbeitslos oder Hilfsspäher
                if (esGibtArbeit())
                {
                    macheArbeit();
                }
                else
                {
                    return;
                }
            }
        }

        // Diese Funktion prüft, ob es Arbeit gibt
        private bool esGibtArbeit()
        {
            if (Kaste == "ZuckerSammler")
            {
                Zucker nähest = Brain.NähesterZucker(this);
                if (nähest != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Kaste == "ObstSammler")
            {
                //Obst-Verbesserung
                if (AktuelleLast != 0)  // Hat Apfel also bereits aufgenommen
                {
                    return true;    // Hier immer true zurückgeben. Behandlung wird dann gesamt in MacheArbeit() gemacht. (Wenn ich hier false zurückgeben würde, würde er Hilfsspäher werden)
                }

                Obst nähest = Brain.NähestesObst(this);
                if (nähest != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Kaste == "Kämpfer")
            {
                Wanze nähest = Brain.NähesteWanze(this);
                if (nähest != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Kaste == "Späher")
            {
                return true;
            }
            else if (Kaste == "Dummy")
            {
                return true;
            }
            return false;
        }

        // Diese Funktion schickt Ameisen zu Arbeit, wenn es welche gibt.
        private void macheArbeit()
        {
            Debug("MacheArbeit()");

            if (Kaste == "ZuckerSammler")
            {
                Zucker nähest = Brain.NähesterZucker(this);
                if (nähest != null)
                {
                    geheDirektZuZiel(nähest);
                }

            }
            else if (Kaste == "ObstSammler")
            {
                // Obst-Verbesserung
                if (AktuelleLast != 0)  // Hat Apfel also bereits aufgenommen
                {
                    Debug("MacheArbeit: Ich hab Last");
                    Obst getragenesObst = Brain.NähestesObst(this); // Hierüber bekomme ich (nicht ganz schön) den nähesten Apfel - welcher der getragene Apfel ist.
                    if (BrauchtNochTräger(getragenesObst) == false)
                    {
                        Debug("Brauche keine Träger mehr");
                        GeheDirektZuBau();
                    }   // ansonst: weiter warten, bis genügend Ameisen da sind.
                    else
                    {
                        Debug("Brauche noch weitere Träger");
                        //BleibStehen();
                    }
                    return;
                }
                Debug("MacheArbeit: Ich habe keine Last");

                Obst nähest = Brain.NähestesObst(this);
                if (nähest != null)
                {
                    geheDirektZuZiel(nähest);
                }

            }
            else if (Kaste == "Kämpfer")
            {
                Wanze nähest = Brain.NähesteWanze(this);
                if (nähest != null)
                {
                    geheDirektZuZiel(nähest);
                }

            }
            else if (Kaste == "Späher")
            {
                GeheGeradeaus();
            }
            else if (Kaste == "Dummy")
            {
                return; // nichts machen.
            }
        }
        #endregion

        #region Sonstiges

        // Debug-Messages :)
        public void Debug(string message)
        {
            return;  // Debugging deaktiviert

            if (Kaste == "Dummy")
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine("Ameise " + id + ": " + message);
        }

        #endregion

        #endregion
    }
}


// kollektives Gedächtnis. Realismus ist was anderes - die Ameisen sind halt modern und haben eine Standverbindung zur OperationsZentrale :-)
public static class Brain
{
    #region diverses

    private static int ameisenZähler = 1;
    public static Spielobjekt Bau = null;

    // AmeisenZähler zurückgeben und erhöhen
    public static int NeueAmeisenID()
    {
        return ameisenZähler++;
    }

    // den Bau setzen
    public static void SetBau(Spielobjekt bau)
    {
        Debug("Bau wurde gesetzt.");
        Bau = bau;
    }

    //Debug Messages :)
    public static void Debug(string s)
    {
        return; // deaktiviert

        System.Diagnostics.Debug.WriteLine("Brain: " + s);
    }

    #endregion

    #region Objektgedächtnis

    private static List<Zucker> zuckerObjekte = new List<Zucker>();
    private static List<Obst> obstObjekte = new List<Obst>();
    private static List<Wanze> wanzeObjekte = new List<Wanze>();

    // ein neues Zucker-Objekt hinzufügen
    public static void MerkeObjekt(Zucker zucker)
    {
        if ((zuckerObjekte.Contains(zucker) == false) && (zucker.Menge != 0)) // wenn das Objekt noch nicht vorhanden ist, und noch Zucker auf dem Berg ist (Zucker existiert weiter, auch wenn er leer ist. Manche Ameisen sehen noch einen leeren Berg.)
        {
            zuckerObjekte.Add(zucker);
            Debug("Objekt '" + zucker + "' nicht vorhanden. Gespeichert.");
        }
    }

    // ein neues Obst-Objekt hinzufügen
    // braucht leider die Ameise, um auf BrauchtNochTräger zugreifen zu können.
    public static void MerkeObjekt(Obst obst, AntMe.Spieler.debuglevel.debuglevelAmeise ameise)
    {
        //Debug("");
        if ((obstObjekte.Contains(obst) == false) && (ameise.BrauchtNochTräger(obst))) // wenn das Objekt noch nicht vorhanden ist
        {
            obstObjekte.Add(obst);
            Debug("Objekt '" + obst + "' nicht vorhanden. Gespeichert.");
        }
    }

    // ein neues Wanze-Objekt hinzufügen
    public static void MerkeObjekt(Wanze wanze)
    {
        if ((wanzeObjekte.Contains(wanze) == false) && (wanze.AktuelleEnergie != 0)) // wenn das Objekt noch nicht vorhanden ist, und Wanze noch nicht tot.
        {
            wanzeObjekte.Add(wanze);
            Debug("Objekt '" + wanze + "' nicht vorhanden. Gespeichert.");
        }
    }

    //Eigenschaften eines Objektes verändern


    // Objekt entfernen
    public static void VergesseObjekt(Obst obst)
    {
        obstObjekte.Remove(obst);
    }

    // Objekt entfernen
    public static void VergesseObjekt(Zucker zucker)
    {
        zuckerObjekte.Remove(zucker);
    }

    // Objekt entfernen
    public static void VergesseObjekt(Wanze wanze)
    {
        wanzeObjekte.Remove(wanze);
    }

    // nähestes Objekt zurückgeben
    public static Obst NähestesObst(AntMe.Spieler.debuglevel.debuglevelAmeise ameise)
    {
        if (obstObjekte.Count == 0)
        {
            return null;
        }

        Obst minimaleDistanz = null;
        for (int no = 0; no < obstObjekte.Count; no++)
        {
            // Wenn das aktuelle Objekt näher an der ameise ist als das vorherige - oder wenn bisher keins eingetragen ist.
            if ((ameise.BrauchtNochTräger(obstObjekte[no])) && (    //nur wenn es noch Träger braucht (macht das Vergessen von Obst überflüssig: Obst existiert zwar noch, hat dann aber immer false bei BrauchtNochTräger())
                (minimaleDistanz == null) ||
                (Koordinate.BestimmeEntfernung(ameise, obstObjekte[no]) <
                Koordinate.BestimmeEntfernung(ameise, minimaleDistanz))))
            {
                minimaleDistanz = obstObjekte[no];
            }
        }
        return minimaleDistanz;
    }

    // nähestes Objekt zurückgeben
    public static Zucker NähesterZucker(AntMe.Spieler.debuglevel.debuglevelAmeise ameise)
    {
        if (zuckerObjekte.Count == 0)
        {
            return null;
        }

        Zucker minimaleDistanz = null;
        for (int no = 0; no < zuckerObjekte.Count; no++)
        {
            // Wenn das aktuelle Objekt näher an der ameise ist als das vorherige - oder wenn bisher keins eingetragen ist.
            if ((minimaleDistanz == null) ||
                (Koordinate.BestimmeEntfernung(ameise, zuckerObjekte[no]) <
                Koordinate.BestimmeEntfernung(ameise, minimaleDistanz)))
            {
                minimaleDistanz = zuckerObjekte[no];
            }
        }

        return minimaleDistanz;
    }

    // nähestes Objekt zurückgeben
    public static Wanze NähesteWanze(AntMe.Spieler.debuglevel.debuglevelAmeise ameise)
    {
        // TODO: Killer-KI ist nicht toll.
        if ((wanzeObjekte.Count == 0) || (Bau == null))
        {
            return null;
        }

        Wanze minimaleDistanz = null;
        for (int no = 0; no < wanzeObjekte.Count; no++)
        {
            // Wenn das aktuelle Objekt näher an der ameise ist als das vorherige - oder wenn bisher keins eingetragen ist.
            if ((minimaleDistanz == null) ||
                (Koordinate.BestimmeEntfernung(Bau, wanzeObjekte[no]) <
                Koordinate.BestimmeEntfernung(Bau, minimaleDistanz)))
            {
                minimaleDistanz = wanzeObjekte[no];
            }
        }

        return minimaleDistanz;
    }


    #endregion
}