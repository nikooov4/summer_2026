if [[ ! -f "appsettings.json" ]]; then
cat <<EOF > appsettings.json
{
  "ConnectionStrings": {
    "TgApiDb": "Host=host; Port=port; Database=database; Username=username; Password=password"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
EOF
fi

if [[ ! -f "appsettings.Development.json" ]]; then
cat <<EOF > appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
EOF
fi
