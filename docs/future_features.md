# Changes planned for version 2.0v

## Feature changes

### General
- Check support for multiple people accessing the platform at the same time without logging out
  the other person — e.g., currently when I log in with one user in one tab, it logs the other
  session out.

- Fix the search system so it allows searching by the item's asset number.

- Create a tab for purchase control of equipment, allowing purchases to be recorded, with the
  invoice number, products, etc. Enable this for admins only.

- Restrict non-admin users to only the equipment transaction and registration parts; editing
  users, editing category properties, etc. should belong to admins only.

### Changes to user management
- Allow password rotation for the `admin.besttechti` user.

- Add validation so that if there is only one admin user, the system doesn't allow deleting that
  user, so it never ends up with no admins registered.

- Check what happens to items registered by a user if that user is deleted — if they lose that
  information, don't allow deleting users at all, only changing them to a "deactivated user"
  status.

### User Management page
- Rename the "name" field label in user editing to "username".

- Allow, when editing a user, setting an existing user as an administrator.

### Register page
- History of changes for each item, such as status changes and notes. Evaluate adding a history
  field.

- Create a section that allows registering new items and changing the information of already
  registered items.

- Add a category property that allows creating an alert on the home screen when an item reaches a
  certain minimum quantity, set in advance.

### Dashboard page
- Make this tab the home screen.
- Change the chart of available equipment in the inventory to pair it with loaned-out equipment.

- Add a separate chart for loaned-out equipment.

### Equipment page

- Create this new tab listing all equipment registered in the system.

- Remove the Items and Loans pages, since it will be possible to filter for that on this new
  page instead.

- Add a column with the quantity of items that match the criteria set by the filters and
  aggregation mechanisms.

- Allow this tab to have 2 mechanisms: filtering and aggregation.
    - Filtering mechanisms: there will be 3 filters:
        - Filter A: filter items by their type — headset, desktop, etc.
        - Filter B: filter the item by its current condition — broken, defective, new, etc.
        - Filter C: filter the item by its current status — loaned out, in stock, etc.
    - Aggregation mechanism: there will be 3 aggregation mechanisms; each combines, into a single
      row of the items table, the quantity of items matching the chosen criteria as a group. For
      example, aggregating by type means each row is broken down only by the items of each type
      (one row for earphones, one for keyboards, etc.), with a count of how many items of that
      type in the quantity column.
        > The aggregation is meant to simulate what an Excel pivot table does, when you want to
        > count how many items have a given property.
        - Mechanism 1: aggregate by item type.
        - Mechanism 2: aggregate by the item's current condition — broken, defective, new, etc.
        - Mechanism 3: aggregate items by their current status — loaned out, in stock, etc.

### Purchase Control page
- Create this tab to manage the registration of new equipment purchases.
