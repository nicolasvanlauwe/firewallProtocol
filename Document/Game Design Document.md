# Game Design Document : Firewall Protocol

**Date :** 14/01/2026

**Moteur :** Unity 2D (C#)

**Genre :** Serious Game / Simulation / Puzzle

---

## 1. Titre et Résumé

### 1.1 Titre du Projet

**Firewall Protocol**

### 1.2 Pitch

"Firewall Protocol" est un Serious Game mobile qui gamifie la sensibilisation à la cybersécurité. Le joueur incarne une personne chargée de protéger le réseau d'une entreprise via une interface rétro simulant un OS des années 2000. Le joueur doit trier un flux incessant d'emails (légitimes vs phishing) en utilisant une mécanique de "Swipe" intuitive (type Tinder), tout en gérant ses ressources et l'amélioration de ses outils via une boutique virtuelle située dans son appartement.

### 1.3 Positionnement

- **Plateforme :** Mobile (Android/iOS) - Mode Portrait.

- **Style Visuel :** Rétro-Computing / Skeuomorphisme (Windows 95/98).

- **Ton :** Stressant (pression temporelle) mais ludique (effets visuels satisfaisants).


---

## 2. Objectifs du Serious Game

### 2.1 Objectifs Pédagogiques

- **Détection du Phishing :** Apprendre à repérer les incohérences (expéditeur suspect, fautes, urgence simulée).

- **Vigilance Numérique :** Transformer la vérification des emails en réflexe conditionné.

- **Gestion du risque :** Comprendre que chaque erreur a un coût pour l'intégrité de l'entreprise.


### 2.2 Objectifs de Gameplay

- **Survie :** Maintenir la jauge d'intégrité de l'entreprise au-dessus de 0% chaque jour.

- **Score & Économie :** Maximiser les gains ("Cryptos") en enchaînant les bonnes réponses (Streak) pour acheter des améliorations. Obtenir le plus haut score.


---

## 3. Public Cible

### 3.1 Profil des utilisateurs

- **Cœur de cible :** Employés d'entreprise (cadres, administratifs, secrétaires) utilisant quotidiennement les emails professionnels.

- **Cible secondaire :** Étudiants et particuliers souhaitant tester leur "culture web" et leur résistance aux arnaques numériques.

- **Niveau d'expérience :** Le jeu s'adresse à tous les niveaux, du débutant complet (qui clique sur tout) à l'utilisateur averti (qui cherche à optimiser son score).


### 3.2 Contexte d'utilisation

- **Micro-learning :** Conçu pour des sessions courtes (5-10 minutes), idéal pour les transports en commun ou les pauses café grâce au format mobile portrait.

- **Formation professionnelle :** Peut être déployé comme module ludique au sein d'un programme de formation en entreprise pour rendre la sensibilisation moins austère.

---

## 4. Gameplay et Mécaniques

### 4.1 Boucle de Jeu (Core Loop)

Le jeu alterne cycliquement entre deux phases principales :

**Phase A : Le Bureau (Action / Journée)**

- **Contexte :** Le joueur est devant son poste de travail virtuel.

- **Action :** Une pile d'emails apparaît. Le joueur doit les traiter un par un (Swipe Gauche/Droite).

- **Fin de phase :** La journée se termine quand tous les emails sont traités ou si l'intégrité tombe à 0 (Game Over).


**Phase B : L'Appartement (Gestion / Repos)**

- **Contexte :** Hub interactif accessible après une victoire (`ApartmentScreen.cs`).

- **Actions possibles :**

	1. **Ordinateur :** Accès à la Boutique pour acheter des améliorations.
 
	2. **Lit :** "Dormir" pour sauvegarder et lancer le jour suivant.

	3. **Porte :** Quitter la partie et retourner au menu principal.


### 4.2 Mécaniques Principales

**Le Swipe (Tri Rapide)**

- Mécanique centrale gérée par `EmailCardUI.cs`.

- **Action :** Glisser la carte vers la droite (Valider/Légitime) ou la gauche (Jeter/Phishing).

- **Feedback :** Une bordure verte ou rouge apparaît dynamiquement selon la distance du glissement.


**Inspection Tactile (Haptic Touch)**

- Appui long sur un élément suspect pour révéler les métadonnées cachées (ex: vraie URL).

### 4.3 Progression et Difficulté

Gérée par `PlayerProgress.cs`.

- **Niveaux (Jours) :** La difficulté augmente chaque jour (introduction de mails "Difficiles" et "Experts").

- **Intégrité :** La barre de vie de l'entreprise diminue au départ de chaque début de jeu (100% -> 60% min), augmentant la tension.

### 4.4 Gamification & Boutique (Shop System) 

Le joueur gagne des "Cryptos" qu'il dépense via l'ordinateur de l'appartement (`ShopSystem.cs`).

| Catégorie         | Item               | Prix | Effet                                 |
| ----------------- | ------------------ | ---- | ------------------------------------- |
| **Consommables**  | Vie Supplémentaire | 100  | Restaure 25% d'intégrité              |
|                   | Indice             | 50   | Révèle si l'email est frauduleux      |
|                   | Passer             | 75   | Passe un email sans pénalité          |
|                   | Bouclier           | 80   | Réduit dégâts prochaine erreur de 50% |
| **Améliorations** | Pare-feu Basique   | 500  | -10% dégâts (permanent)               |
|                   | Pare-feu Avancé    | 1000 | -20% dégâts (requiert basique)        |
|                   | Bonus Coins +10%   | 300  | +10% coins gagnés                     |
|                   | Bonus Coins +25%   | 800  | +25% coins (requiert +10%)            |
|                   | Intégrité +10      | 600  | +10 intégrité au départ               |
|                   | Gardien de Série   | 750  | Erreur ne reset plus la série         |

![[graph.png]]

| **Jour** | **Joueur Parfait (Coins Total)** | **Joueur Bon (Coins Total)** | **Joueur Moyen (Coins Total)** | **Objectif d'achat possible (ShopSystem.cs)** |
| -------- | -------------------------------- | ---------------------------- | ------------------------------ | --------------------------------------------- |
| **1**    | **300**                          | **125**                      | **94**                         | _Vie Extra (100), Indice (50)_                |
| **2**    | 649                              | 287                          | 225                            | _Bonus Coins +10% (300)_                      |
| **3**    | 1053                             | 494                          | 360                            | _Pare-feu Basique (500)_                      |
| **4**    | 1518                             | 751                          | 543                            | _Intégrité +10 (600)_                         |
| **5**    | 2050                             | 1063                         | 765                            | _Gardien de Série (750)_                      |
| **7**    | 3339                             | 1767                         | 1347                           | _Pare-feu Avancé (1000)_                      |
### 4.4 Interactions

Le jeu se joue en solo sans multijoueur ou coopération possible. En revanche il garde la partie du joueur enregistré s'il souhaite la continuer plus tard.

---
## 5. Narration et Contexte

### 5.1 Synopsis

Le joueur est une nouvelle recrue (ou une I.A. potentiellement) au sein du département sécurité d'une entreprise. Au fil des jours, les emails deviennent de plus en plus dur à analyser avec des pièges encore plus sournois.

### 5.2 Personnages

- **Le Joueur (Héros invisible) :** L'opérateur derrière l'écran.

- **L'Expéditeur (PNJ abstrait) :** Représenté par les emails (Le Prince Nigérian, Le PDG impatient, La RH débordée).


### 5.3 Univers

- **Ambiance Visuelle :** "Rétro-Computing" / Skeuomorphisme. Imitation d'un OS Windows 95/98 (fenêtres grises, barre des tâches, fond d'écran colline typique).

- **Lieu :** Un bureau virtuel et un appartement vue de dessus (`ApartmentScreen.cs`).

---
## 6. Contenu et Données (Data)

### 6.1 Structure des Emails (JSON)

Le contenu n'est pas codé en dur. Il est chargé dynamiquement via `EmailLoader.cs` depuis un fichier JSON.

Chaque email possède :

- `expediteurNom` / `expediteurEmail`

- `objet` / `corpsDuMessage`

- `estFrauduleux` (booléen)

- `difficulte` (facile, moyen, difficile, expert)

- `explicationErreur` (Feedback pédagogique)

- `pointsSiCorrect` / `degatsIntegrite` (score gagnée / dégâts infligés)


### 6.2 Types de menaces

- **Facile :** Arnaques évidentes (Prince, Orthographe déplorable).

- **Moyen :** Fausses promotions, alertes de sécurité génériques.

- **Difficile :** Spear-phishing (ciblé), Faux email du PDG (CEO Fraud), Ransomware déguisé en facture.


---

## 7. Technologie et Architecture

### 7.1 Moteur et Langage

- **Moteur :** Unity (2D).

- **Langage :** C#.


### 7.2 Architecture du Code

- **Singletons :** `GameManager`, `ShopSystem`, `PlayerProgress`, `EffectsManager` pour une accessibilité globale.

- **Persistance :** Sauvegarde automatique via `PlayerPrefs` directement intégré dans Unity (Sérialisation JSON de l'objet `PlayerProgress`).

- **Animation :** Utilisation de la librairie **DOTween** pour toutes les interpolations (UI, Cartes, Glitch). Importation de police, etc... pour le design.


### 7.3 Assets Techniques

- **EmailJSON / EmailData :** Classes pour le parsing des emails depuis le fichier JSON (`Resources/emails.json`). `EmailJSON` contient le champ `difficulte` pour la sélection par niveau, `EmailData` est la version simplifiée utilisée en jeu.

- **EmailDatabase :** Container pour `JsonUtility.FromJson()`, gère le chargement et le filtrage des emails par difficulté.
- ```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              RÉSUMÉ DU FLUX                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   emails.json                                                               │
│       │                                                                     │
│       ▼                                                                     │
│   EmailDatabase.LoadFromJSON()                                              │
│       │                                                                     │
│       ▼                                                                     │
│   List<EmailJSON>  ──(stocké dans EmailLoader.database)                     │
│       │                                                                     │
│       ▼                                                                     │
│   EmailLoader.PrepareGameForDay(jour)                                       │
│       │                                                                     │
│       ├── Filtre par difficulté                                             │
│       ├── Applique les % du DayConfig                                       │
│       ├── Mélange                                                           │
│       └── Convertit via ToEmailData()                                       │
│       │                                                                     │
│       ▼                                                                     │
│   List<EmailData>  ──(utilisé par GameManager)                              │
│       │                                                                     │
│       ▼                                                                     │
│   GameManager parcourt la liste et affiche chaque email                     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

- **ShopItem :** Classe simple (non sérialisable) définissant les items de la boutique. Les items sont définis en dur dans `ShopSystem.cs` via une liste `static readonly`.

- **DayConfig :** Structure définissant la configuration de chaque jour (nombre d'emails, répartition des difficultés, intégrité de départ). Définie en dur dans `PlayerProgress.cs`.

- **Prefabs :** `ShopItem.prefab` est le prefab UI pour l'affichage visuel d'un item dans l'interface de la boutique (instancié par `ShopUI.cs`).


### 7.4 Architecture Backend et API (Historique des parties)

Le jeu utilise une **API REST** pour assurer la persistance des données du joueur. L'objectif unique est de sauvegarder l'historique personnel pour que le joueur puisse suivre sa propre évolution.

- **Identification et Confidentialité :**

    - L'authentification repose sur l'association d'un **Pseudo** (choisi au lancement) et de l'**Identifiant Unique de l'Appareil** (`Device ID`).

    - **Cloisonnement des données :** L'API est configurée pour ne renvoyer à l'application que les données strictement liées à l'ID de l'appareil appelant. Aucun joueur ne peut voir les scores des autres.

- **Cycle de Sauvegarde (Archivage) :** Puisqu'une nouvelle partie (New Game) écrase la progression en cours, l'API sert de "mémoire". À la fin d'une partie, le client envoie une requête `POST` pour archiver la tentative :

    1. **Niveau atteint :** Le jour où la partie s'est arrêtée.

    2. **Score Final :** La performance globale.

    3. **Raison de l'arrêt :** (ex: "Game Over" ou "Abandon").

- **Structure des Données (JSON) :** Exemple de log envoyé en fin de partie :

`{ "player_pseudo": "Agent_Neo", "device_id": "unique-device-id-123", "day_reached": 8, "final_score": 4500, "end_reason": "GAME_OVER_INTEGRITY" }`

---

## 8. Système de Feedback (Juice)

Le jeu met l'accent sur le ressenti utilisateur ("Game Feel") géré par `EffectsManager.cs`.

### 8.1 Feedback Positif

- **Son :** Bruit de validation.

- **Visuel :** Animation de rebond ("Bounce") de la carte.

- **Victoire :** Lancer de confettis (`ConfettiEffect.cs`) à la fin de la journée.

### 8.2 Feedback Négatif

- **Visuel :** Effet de **Glitch** (`GlitchEffect.cs`) qui déforme l'écran lors d'une erreur.

- **Game Over :** Glitch suivi d'un fondu au noir et d'un écran de rapport d'erreur système.

### 8.3 Feedback Pédagogique

- Lorsqu'une erreur est commise, une **Popup** (`FeedbackPopup.cs`) apparaît expliquant précisément pourquoi le joueur s'est trompé (ex: "L'email expéditeur n'était pas le vrai mail de l'entreprise en question.").

### 8.4 Suivi de Progression

Grâce à la connexion API, le joueur a accès à un écran **"Journal d'Activité"** (accessible depuis l'Appartement ou le Menu Principal).

- **Visualisation :** Le joueur consulte la liste de ses 10 dernières tentatives.

- **Objectif d'auto-amélioration :** Le système met en évidence les meilleures performances personnelles (High Score) pour encourager le joueur à battre son propre record.

- **Intérêt Pédagogique :** Cela permet au joueur de voir s'il survit plus longtemps au fil des essais (ex: "Il y a 3 jours je perdais au Jour 2, aujourd'hui j'arrive au Jour 5").

---
# *Réalité*
## 9. Contraintes et Faisabilité

### 9.1 Durée de développement

- Projet étudiant : 3 mois.

### 9.2 Ressources nécessaires

- 1 Développeur Unity.

- Assets graphiques (UI Rétro libre de droits ou création simple).

- Rédaction des scénarios d'emails (JSON).

### 9.3 Risques

- **Répétitivité :** Risque que le joueur s'ennuie, redondance des mails au fur et à mesure des parties. _Solution :_ Varier les scénarios, augmenter la difficulté et ajouter de nouveaux mails. Ajouter des évènements pendant le jeu afin de le dynamiser.

- **Lisibilité sur mobile :** Texte des emails trop petit. _Solution :_ UI adaptative et textes courts.

- **Équilibrage :** Items dans la boutique pas assez chère. Trop d'intégrité, impossible de perdre. Mails trop facile à cerner. _Solution :_ Rectifier au fur et à mesure suivant les retours.


---

## 10. Plan de Lancement

### 10.1 Stratégie

- **Déploiement :** Build Android (APK).

- **Mise à jour :** Ajout facile de nouveaux "Packs d'emails" via le fichier JSON sans recompiler le jeu complet, nouveaux items dans la boutique.

---
# *Entreprise*

## 9. Contraintes et Faisabilité

### 9.1 Durée de Développement

**Estimation totale : 6 mois** (pour une version commerciale finie).

- **Mois 1 : Prototype (Pré-production)**

    - Validation des mécaniques (le Swipe) et de la direction artistique.

    - _Livrable :_ Une version jouable "moche" avec 10 emails pour tester le fun.

- **Mois 2 à 4 : Production**

    - Création de tous les assets graphiques (UI rétro, icônes).

    - Rédaction des 100+ scénarios d'emails (fichier JSON).

    - Développement de la boutique et du système de sauvegarde.

- **Mois 5 : Tests et Corrections (QA)**

    - Chasse aux bugs sur différents téléphones Android.

    - Ajustement de la difficulté (équilibrage de l'économie).

- **Mois 6 : Finalisation**

    - Ajout des bruitages et musiques.

    - Préparation de la page store (Google Play).


### 9.2 Budget Estimé

On simule ici le coût réel si on devait payer l'équipe. **Budget Total : ~60 000 €**

- **Équipe de développement (Salaires) : 50 400 €**

    - Calculé sur la base de 3 personnes (1 Développeur, 1 Graphiste, 1 Game Designer) travaillant pendant 6 mois avec des salaires junior/intermédiaire.

    - **Volume horaire :** 840 heures par personne (24 semaines).

    - Calculé sur la base de 3 profils juniors à temps plein (35h/semaine) avec un coût chargé d'environ 20€/h (16 800€ x 3)

------
-  **Matériel et Logiciels (Infrastructure) : 4 500 €**

	- **Licences Logicielles (1 500 €) :**

	    - Unity Pro (3 sièges).

	    - Outils graphiques et Hébergement Git.

	- **Parc de Test Mobile (2 900 €) :**

	    - Il est crucial de tester sur une large gamme d'appareils Android pour garantir la stabilité :

	        - 1 Appareil "Entrée de gamme" (ex: Vieux Xiaomi/Wiko) pour tester l'optimisation.

	        - 1 Appareil "Standard" (Samsung Galaxy A series).

	        - 1 Appareil "Format exotique" (Tablette ou écran très allongé) pour vérifier l'ancrage de l'UI.

	- **Frais de Publication (Environ 100 €) :**

	    - **Google Play Console :** 25 € (Paiement unique à vie, contrairement à Apple qui est un abonnement annuel).

	    - Frais administratifs divers.

- **Marketing et Acquisition : 5 000 €**

	- **Site Web Vitrine :** 1 000 €

	- **Campagne LinkedIn & Google Ads :** 4 000 €

	    - Ciblage spécifique des appareils Android professionnels.



### 9.3 Ressources Nécessaires

- **Ressources Humaines :**

    - **1 Développeur Unity :** S'occupe du code C#, de l'intégration et des bugs.

    - **1 Artiste 2D :** S'occupe du style visuel "Windows 98" et des effets.

    - **1 Game Designer / Scénariste :** Écrit les emails de phishing pour qu'ils soient réalistes.

- **Ressources Techniques :**

    - Gestion de projet : Trello (pour suivre les tâches).

    - Sauvegarde du code : Git (GitHub ou GitLab).

    - Serveur simple : Pour héberger le fichier des emails (JSON) et pouvoir le modifier à distance.    

### 9.4 Risques Principaux

- **Risque : Le jeu est ennuyeux à la longue.**

    - _Solution :_ Ajouter des événements aléatoires (coupure de courant, virus agressif) pour briser la routine.

- **Risque : Le jeu ne marche pas sur les vieux téléphones.**

    - _Solution :_ Optimiser le code pour qu'il soit léger (pas de 3D, graphismes simples).


---

## 10. Plan de Lancement et Évaluation

### 10.1 Stratégie de Lancement

Le but est de vendre le jeu à des entreprises pour former leurs employés.

1. **Lancement "Pilote" (Test) :**

    - On propose le jeu gratuitement à une seule entreprise partenaire (ex: une PME locale de 50 employés).

    - On observe s'ils aiment le jeu et s'ils rencontrent des bugs.

2. **Lancement Commercial :**

    - Mise en ligne sur les stores (Google Play / App Store) en accès privé ou public.

    - Vente de "licences" aux entreprises (ex : 10€ par employé pour un accès illimité).


### 10.2 Évaluation du Succès (KPI)

Comment savoir si le projet est une réussite ? On regarde ces indicateurs :

- **Taux de participation :** Est-ce que les employés téléchargent le jeu quand on leur propose ?

- **Taux de complétion :** Combien de joueurs vont jusqu'au bout des 10 jours de jeu ? (Si tout le monde arrête au jour 2, le jeu est trop dur ou ennuyeux).

- **Score moyen :** Est-ce que les joueurs s'améliorent ? (Moins d'erreurs au jour 10 qu'au jour 1).


### 10.3 Suivi et Mises à jour

Une fois le jeu sorti, le travail n'est pas fini :

- **Mises à jour de contenu :** Ajouter de nouveaux emails tous les mois pour coller à l'actualité des arnaques (ex: arnaques aux JO, arnaques aux impôts).

- **Support technique :** Répondre aux utilisateurs qui ont des problèmes d'installation.

---

## 11. Annexes

### 11.1 Croquis 


**Phase de jeu**

![[sketch_mail.jpg]]


**Appartement**

![[sketch_app.jpg]]!

**Boutique**

![[sketch_shop.jpg]]
