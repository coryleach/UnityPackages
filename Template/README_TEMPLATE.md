<h1 align="center">{PACKAGE.NAME} 👋</h1>
<p>
  <img alt="Version" src="https://img.shields.io/badge/version-{PACKAGE.VERSION}-blue.svg?cacheSeconds=2592000" />
  <a href="https://twitter.com/{AUTHOR.TWITTER}">
    <img alt="Twitter: {TWITTER.USERNAME}" src="https://img.shields.io/twitter/follow/{TWITTER.USERNAME}.svg?style=social" target="_blank" />
  </a>
</p>

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

## Usage

{PACKAGE.USAGE}

## Author

👤 **{AUTHOR.NAME}**

{AUTHOR.SOCIAL}

## Show your support

Give a ⭐️ if this project helped you!

***
_This README was generated with ❤️ by [readme-md-generator](https://github.com/kefranabg/readme-md-generator)_
