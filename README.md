# YouTube Downloader v2

## Spis treści
* [Opis](#opis)
* [Technologie](#technologie)
* [Cel powstania aplikacji](#cel-powstania-aplikacji)
* [Możliwości aplikacji](#możliwości-aplikacji)
* [Problemy](#problemy)
* [Kontakt](#kontakt)

## Opis
Aplikacja służąca do pobierania filmów i plików dźwiękowych z serwisu YouTube. Prace nad tym projektem rozpoczęły się w sierpniu 2018 roku i ciągle trwa jej rozwój. Dodatkowo aplikacja korzysta z biblioteki YouTubeExplode

## Technologie
* C#
* WPF
* .NET 4.6.1
* YouTube Data API v3

## Cel powstania aplikacji
Aplikacja powstała na własne potrzeby. Wiele jest ogólnodostępnych programów pozwalających pobierać filmy i/lub muzykę z YouTube, natomiast ciężko było mi znalęźć taką, która byłaby jednocześnie darmowa i pozwalała na pobieraniech całych playlist. Z tego faktu wydawało mi się, że niejednej osobie przyda się taka aplikacja, a przy okazji będzie to dobra okazja do nauczenia się nowych rzeczy.

## Możliwości aplikacji
<p>Aplikacja posiada trzy zakładki: Home, Playlists oraz Settings</p>

#### Home
W trybie Home można pobierać pojedyńcze filmy. W tym celu należy wkleić link do filmu i zaczekać aż wyświetlą się podstawowe informacje na temat video. Po pomyślnym załadowaniu filmu należy zaznaczyć czy chcemy pobrać tylko dźwięk oraz czy pobieramy do domyślnej ścieżki

#### Playlists
Najbardziej rozbudowany tryb aplikacji. W tym miejscu możemy pobierać całe lub wybrane video ze wskazanej playlisty. Podobnie jak w przypadku zakładki Home, tutaj również zaczynamy od wklejenia Url-a interesującej nas playlisty. Następnie wyświetli się okno wybrania utworów, które chcemy pobrać. Zaznaczamy te, które nas interesują i klikamy Download. Pobrana przez nas playlista dodała się do listy "obserwowanych". Co nam daję możliwość obserwowania? <br>
* widzimy historię, które playlisty pobieraliśmy
* w przypadku dodania nowych video do playlisty możemy ją zaaktualizować i pobrać te utwory, których jeszcze nie pobieraliśmy
* możemy ustawić cykliczne odświeżanie playlisty, aby być zawsze na bieżąco
<p>Aby usunąć playlistę z listy, należy nacisnąć prawy przycisk myszy na wybranej pozycji i wybrać opjcę Delete.</p>

#### Settings
Tutaj znajdziemy ustawienia aplikacji. Możemy ustawić domyślną ścieżkę pobierania, do której bedą się pobierać utwory pobierane z zakładki Home lub, w której bedą się tworzyć katologi playlist w przypadku zakładki Playlists. Dodatkowo możemy ustawić czy chcemy, aby aplikacja otwierała się razem ze startem Windows na naszym komputerze


## Problemy
<p>Z racji tego, że YouTube walczy z pobieraniem utworów z ich serwisu non stop wprowadzane są nowe zabezpieczenia, które mogą uniemożliwić pobranie niektórych filmów, dlatego miej na uwadzę aby zawsze być w posiadaniu najnowszej wersji aplikacji</p>

## Kontakt
<p> W razie jakichkolwiek pytań proszę się kontaktować pod mailem <a href="mailto: lykoszczan@gmail.com">lykoszczan@gmail.com</a></p>
