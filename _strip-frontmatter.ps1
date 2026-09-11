# Strip ticket-related frontmatter fields and ticket references from all notes
# Also remove "Ticket X" references, "blocked_by", "blocks", "date_completed", "status", "type", "tags" from frontmatter
# And remove sections that reference tickets

Get-ChildItem "docs\notes\*.md" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw

    # Remove ticket-related frontmatter lines
    $content = $content -replace "(?m)^ticket:.*$\r?\n", ''
    $content = $content -replace "(?m)^type:.*$\r?\n", ''
    $content = $content -replace "(?m)^date_completed:.*$\r?\n", ''
    $content = $content -replace "(?m)^status:.*$\r?\n", ''
    $content = $content -replace "(?m)^blocked_by:.*$\r?\n", ''
    $content = $content -replace "(?m)^blocks:.*$\r?\n", ''
    $content = $content -replace "(?m)^tags:.*$\r?\n", ''

    # Remove "Ticket X" references in body text
    $content = $content -replace 'Ticket \d+', ''
    $content = $content -replace '\(Ticket \d+\)', ''
    $content = $content -replace 'ticket \d+', ''
    $content = $content -replace 'T03', ''

    # Remove "ticket-completion" tag references
    $content = $content -replace 'ticket-completion, ?', ''
    $content = $content -replace 'ticket-completion', ''

    # Remove empty frontmatter (if only title remains, that's fine, but clean up empty frontmatter)
    # Remove "## 📝 Artifacts Created" and "## 📝 Artifacts Modified" sections (ticket-specific)
    $content = $content -replace '(?s)(## 📝 Artifacts Created\r?\n.*?)(?=\n## |\Z)', ''
    $content = $content -replace '(?s)(## 📝 Artifacts Modified\r?\n.*?)(?=\n## |\Z)', ''

    # Remove "PLAN.md" references
    $content = $content -replace 'PLAN\.md', ''

    # Remove "tracker/" references  
    $content = $content -replace 'tracker/tickets/.*\.md', ''
    $content = $content -replace 'tracker/MAP\.md', ''

    # Clean up any double blank lines
    $content = $content -replace '(\r?\n){3,}', "`n`n"

    Set-Content $_.FullName -Value $content -NoNewline
    Write-Host "Stripped: $($_.Name)"
}
