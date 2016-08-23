# Gestion droits

[![Build Status](https://ci.appveyor.com/api/projects/status/github/Darkitty/GestionDroits)](https://ci.appveyor.com/project/Darkitty/gestiondroits)
[![Coverage Status](https://coveralls.io/repos/github/Darkitty/GestionDroits/badge.svg?branch=master)](https://coveralls.io/github/Darkitty/kermene?branch=master)

_Langage : ```C#``` - Date : 06/06/2016_

Application permettant la gestion des membres dans les groupes AD. L'ensemble des ```OU``` est défini dans le fichier ```GestionDroits.exe.config``` ainsi que le nom de domaine.

|Key|Value|
|---|-----|
|pathDoc|Chemin absolu du fichier de documentation|
|domain|Nom du domaine|
|mailserver|Nom ou IP du serveur de mail|
|mail|Adresse mail servant à l’envoie des courriels|
|list|Liste des noms à afficher et à séparer par des « ; »|
|nom1|Chemin de l’OU voulu (ex : OU=Groupes ;dc=domain ;dc=tld)|

## Gestion des membres

![](https://lut.im/05HdNMI6YX/qBKArOmsDuyc4Tud)

Pour chaque OU sélectionée dans la liste, l'ensemble des groupes sera ensuite affiché en dessous. Il y aura alors plusieurs cas possible :

* Vous n'êtes ni membre ni propiétaire du groupe : vous aurez seulement le prénom et nonm du propiétaire du groupe si celui-ci est défini
* Vous être membre : vous pouvez effectuer une demande d'ajout d'un nouveau membre. Un mail sera alors envoyé au responsable pour lui signaler votre demande
* Vous être le propriétaire : la liste de l'ensemble des membres vous est visible. Vous pouvez en supprimer en sélectionnant un utilisateur puis ```Supprimer```, en ajouter un nouveau en le sélectionnant dans la liste puis ```Ajouter```.

## Détails sur un utilisateur

![](https://lut.im/262LU9sG6y/s7pCqIArIGMzJqAc)

Cette fenêtre permet d'afficher un ensemble d'information ainsi que l'ensemble des groupes dont l'utilisateur sélectionné est membre. Aucune modification n'est cependant possible.