Im Output Folder dieses Projekts wird ein NUGET Package erstellt.
Dieses kann dann mit Hilfe der NugetPackageExplorer Anwendung auf einen NUGet Server hochgeladen (Menüpunkt 'Publish...') werden.
hochzuladen ist die Datei mit der Endung .symbols.nupkg


Derzeitiger EntwicklungServer dafür ist:

URL:          http://v-w8r2srv-01.rkos/NuGetServer/
Publish Key:  12345678-90ab-cdef-1234-567890abcdef


New Features iin Version 1.0.4932.25260  (3.7.2013)
  * Neues Base Model: BaseEditableViewModel  mit leerer Implementierung von IEditableObject
  * Eine gesammelte Methode für alle Optionen des Raise Proprty change. (Alle anderen sind Obsolete)
  * OnChanged() nach aussen geben.


New Features in Version 1.0.4830.20575   (23.3.2013)
  * RelayCommand auf sparsamere Version umstellen http://www.mycsharp.de/wbb2/thread.php?postid=3727481#post3727481
  * Eventuell Copyright Hinweise in Klassen (RelayCommand,?) anbringen ? (Falls Source code an WLB geht !?)
  * Die Tree View basisklasse aus dem LagerabrechnungViewModel Projekt übernehmen.
  * logging auf TraceSourceumstellen. (Common Logging Interface dazu?)
  * MVVMBase soll ein Error-Event bekommen.


New Features in Version 1.0.4841.26152 (erldigte TODOs)...

  * ViewModelError im Dispose auf Null setzen.
  * Extension-Method für ObserveableCollection<> 'ClearAndDispose()'
  * Logging nur mehr wenn EventHandler vorhanden sind und zuhören.

TODO List für nächste Version der MvvmBase
 
  * Test für den TreeView dazu.
  * Initialized Methode auf public und eventuell virtual?
  