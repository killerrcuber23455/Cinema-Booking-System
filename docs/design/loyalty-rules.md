# Loyalty rules (card 329)
<!-- owner: Divyesh. Destination: docs/design/loyalty-rules.md -->

Status: lines marked CONFIRMED were agreed with Shankar and Akilesh. Lines marked TO CONFIRM are my defaults, tell me by Sunday if you disagree.

Note: every number below (rates, limits, discounts, points value) is the default. The Admin can change tier rules in the Admin windows (file 27), so the code must read them from one place and never type them inside forms.

## Earning
- 1 point per Rs 10 paid. CONFIRMED (rate)
- Points are rounded down to a whole number. CONFIRMED
  Example: Standard, Rs 295 paid gives 29 points.
- Silver earns 1.25 times points, Gold 1.5 times. Work out paid / 10 x multiplier, then round down. CONFIRMED (card 329)
  Example: Gold, Rs 295 paid gives 29.5 x 1.5 = 44.25, so 44 points.
- Points are worked out on the amount actually paid, after any points discount. TO CONFIRM
- Points are added only when the booking is confirmed. TO CONFIRM
- Only members earn points. A guest booking earns and uses 0 points. A member booked by Staff or Admin still earns points on their own account. TO CONFIRM

## Tiers
- Standard: 0 to 99 lifetime points. Silver: 100 to 299. Gold: 300 and above. CONFIRMED
- Lifetime points count every point ever earned. Using points does not lower them. TO CONFIRM
- Tier goes up automatically as soon as lifetime points reach the limit. CONFIRMED (card 329)
- Tier discount on the ticket price: Standard 0%, Silver 5%, Gold 10%. CONFIRMED (card 329)
- Tier is recalculated after every cancel too. TO CONFIRM

## Order of working out a booking
1. Ticket total.
2. Take off the tier discount (5% or 10%).
3. Take off points discount (100 points = Rs 50), never below Rs 0.
4. Amount paid = what is left. Points earned come from this amount. CONFIRMED

Using points at booking is card 330 (Sprint 4). Until then PointsUsed stays 0.

## Using points
- 100 points = Rs 50 off. CONFIRMED
- Points can only be used in multiples of 100. CONFIRMED
- The discount can never be more than the booking total (the price cannot go below Rs 0). TO CONFIRM
  Example: total Rs 120, balance 500 points: at most 200 points (Rs 100) can be used.
- A member cannot use more points than their balance. TO CONFIRM

## Cancelling a booking
- Points used on that booking are given back. TO CONFIRM
- Points earned from that booking are taken back. TO CONFIRM
- Balance never goes below 0. CONFIRMED
- Lifetime points go down by the points taken back (never below 0), so the tier can go down. TO CONFIRM

## Other confirmed decisions
- Showing price 0 is not allowed, price must be above 0.
- Showing start time in the past is not allowed.
- Overlapping showings on the same screen are not allowed.
- Seat booking is all or nothing: if one seat fails, none are booked.
- Password minimum length is 8 characters.

## Test examples (for Akilesh / Irwin)
| Case | Paid | Result |
|---|---|---|
| Earn | Rs 295 | +29 points |
| Earn | Rs 9 | +0 points |
| Silver earn | Rs 200 | 20 x 1.25 = 25 points |
| Gold earn | Rs 200 | 20 x 1.5 = 30 points |
| Silver discount | Rs 200 | pays Rs 190 |
| Gold discount | Rs 200 | pays Rs 180 |
| Reach Silver | lifetime 100 | Silver |
| Reach Gold | lifetime 300 | Gold |
| Use 150 points | any | refused, not a multiple of 100 |
| Use 100 points on Rs 40 total | Rs 40 | refused or capped, total cannot go below 0 |
| Cancel with balance 20, earned 29 from it | | balance 0, not -9 |
