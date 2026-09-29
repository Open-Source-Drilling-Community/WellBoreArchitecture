# WebPages regression tests

Tests invoke the simplified architecture editor mutation callbacks with independent host-relative and vertical coordinates. They verify that editing or clearing a connector vertical depth preserves its local position and uncertainty. No browser, network endpoint or persistent database is required.

Run `dotnet test WebPagesTest/WebPagesTest.csproj -c Release`.
