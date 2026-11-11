# Uploading this project to GitHub

This ZIP contains a standalone ASP.NET Core MVC project inside the `MyPharmacy` folder. The current repository has an older application at its root, so the safest first upload is to add this as a separate folder. This preserves the team's existing files and lets everyone compare the new implementation before replacing anything.

## 1. Extract and test the new project

1. Make a backup of your current local repository.
2. Extract this ZIP.
3. Copy the complete `MyPharmacy` folder from the extracted ZIP into the root of your local repository. Do not copy its contents over the existing root files.
4. Open a terminal in the new folder and run:

```bash
cd MyPharmacy
dotnet restore
dotnet build
dotnet run
```

5. Open `https://localhost:7180`. If the HTTPS certificate is not trusted, run `dotnet dev-certs https --trust` and restart the application.
6. Test both demo accounts listed in `README.md` before uploading.

## 2. Commit and push to your feature branch

From the repository root:

```bash
git checkout Melany
git pull origin Melany
git status
git add MyPharmacy
git commit -m "Add English MyPharmacy project with Identity authentication"
git push origin Melany
```

Do not push directly to `main`. Open a pull request after the team reviews the new folder and confirms whether it should replace the older root-level implementation.

## If the team decides to replace the old root-level project

Do this only after the new folder builds and the team has reviewed it. Back up the old project first, then plan the replacement carefully so that the old `SistemaFarmacia.csproj` and old source files do not remain alongside the new `MyPharmacy.csproj` in the same build directory.
