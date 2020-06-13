<h1 align="center">{PACKAGE.DISPLAYNAME} 👋</h1>

<!-- BADGE-START -->
![version](https://img.shields.io/github/package-json/v/{GITHUB.USERNAME}/{PACKAGE.REPOSITORYNAME})
[![license](https://img.shields.io/github/license/{GITHUB.USERNAME}/{PACKAGE.REPOSITORYNAME})](https://github.com/{GITHUB.USERNAME}/{PACKAGE.REPOSITORYNAME}/blob/master/LICENSE)
[![twitter](https://img.shields.io/twitter/follow/{TWITTER.USERNAME}.svg?style=social)](https://twitter.com/{TWITTER.USERNAME})
<!-- BADGE-END -->

{PACKAGE.DESCRIPTION}

## Quick Package Install

#### Using UnityPackageManager (for Unity 2019.3 or later)
Open the package manager window (menu: Window > Package Manager)<br/>
Select "Add package from git URL...", fill in the pop-up with the following link:<br/>
{PACKAGE.URL}<br/>

#### Using UnityPackageManager (for Unity 2019.1 or later)

Find the manifest.json file in the Packages folder of your project and edit it to look like this:
```js
{
  "dependencies": {
    "{PACKAGE.NAME}": "{PACKAGE.URL}",
    ...
  },
}
```

<!-- DOC-START -->
<!-- 
Changes between 'DOC START' and 'DOC END' will not be modified by readme update scripts
-->

## Usage
{PACKAGE.USAGE}

<!-- DOC-END -->

## Author

👤 **{AUTHOR.NAME}**

{AUTHOR.SOCIAL}

## Show your support

Give a ⭐️ if this project helped you!

***
_This README was generated with ❤️ by [Gameframe.Packages](https://github.com/coryleach/unitypackages)_
