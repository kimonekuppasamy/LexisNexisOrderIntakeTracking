## Assumptions
- No real authentication. Signing in means typing an email. There are no passwords, because auth was out of scope.
- Admins are ordinary customers with IsAdmin = true in the data.
- Single user, local use. The app runs on one machine for demos, so there is no handling for many people saving at the same time.
- The cart is temporary. It lives in browser memory and is cleared when the page is refreshed.
- Only admins change order statuses. Customers can't cancel their own orders.
- An order keeps a snapshot of each product, so later price changes don't change old orders.
- The cart always shows the current product price (refreshed when the cart opens). If a price changes before the order is submitted, the server rejects the order with a message naming the product, and the cart refreshes so the customer can review and resubmit.

## Future Improvements

- Replace JSON file storage with a relational database
- Add authentication and authorization 
- Add proper role-based access control
- Add pagination and filtering (looked at AG Grid, but due to time constraints I did not implement this)
- Add more observability both in frontend and backend
- Add deployment configuration


## Example Order

A typical order progresses through the following lifecycle:

1. Customer adds flowers
2. Customer reviews cart
3. Customer submits order
4. Order moves from Pending
5. To Confirmed
6. To Shipped
7. To Delivered

An order can be cancelled while it is still Pending or Confirmed.

## AI Usage

- Create tests using business logic provided once I had my app structures and basic logic so I could see any gaps:
Prompt -> 
Generate unit tests based on the following business logic and criteria, ensure to list any gaps from my existing structure so that I may build :
When the same client-provided external reference is submitted again, the system avoids creating duplicate orders and behaves
consistently from a user’s point of view.
• Quantities are positive whole numbers; prices are non-negative.
• Line totals and overall totals are calculated on the server.
• Support basic status changes that make sense for a simple sales flow. If a change doesn’t make sense, provide a helpful message.
• Provide a way to list orders showing the most recent ones first.
• You design the API, data model, validation, and error behavior.
Accepts new orders and handles repeat submissions of the same client reference without creating duplicates.
• Computes totals on the server.
• Supports retrieving a single order and listing orders with newest first.
• Supports changing an order’s status with sensible rules and helpful feedback when a change isn’t allowed.