# Convert a json into a collection initializer for the Intervals.cs model

$response = Invoke-RestMethod https://macrostrat.org/api/v1/defs/intervals?timescale=international%20intervals

$response.success.data | ForEach-Object {

    $line = [System.Collections.ArrayList]@(
        "new Interval {",
        "Name=`"$($_.name)`",",
        "Abbrev=`"$($_.abbrev)`",",
        "Early_Age=$($_.early_age),",
        "Late_Age=$($_.late_age),",
        "Type=`"$($_.type)`",",
        "Color=`"$($_.color)`"",
        "},"
    ) 

    Write-Host $line
}