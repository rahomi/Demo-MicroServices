$oldNew = @{
    '01-scaffold-solution' = 'solution-scaffolding-and-contracts'
    '02-products-service' = 'cqrs-with-mediatr'
    '03-baskets-service' = 'basket-operations-and-event-consumer'
    '04-identity-service' = 'minimal-service-boundary'
    '05-orders-service' = 'order-submission-and-events'
    '06-notifications-service' = 'event-driven-consumer-pattern'
    '07-bff-service' = 'backend-for-frontend-pattern'
    '08-saga-orchestration' = 'saga-pattern-with-compensation'
    '09-distributed-tracing' = 'distributed-tracing-with-opentelemetry'
    '10-docker-compose' = 'docker-compose-and-containerization'
    '11-http-examples-and-readme' = 'api-documentation'
    'T03-rabbitmq-contracts-design' = 'rabbitmq-messaging-topology'
}

Get-ChildItem "docs\notes\*.md" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    foreach ($key in $oldNew.Keys) {
        $newVal = $oldNew[$key]
        $content = $content -replace [regex]::Escape("[[$key"), "[[$newVal"
    }
    # Remove [[retrospective]] links entirely
    $content = $content -replace '\[\[retrospective\]\]', ''
    Set-Content $_.FullName -Value $content -NoNewline
    Write-Host "Updated: $($_.Name)"
}
