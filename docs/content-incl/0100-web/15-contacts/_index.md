---
title: "Contact Migration"
weight: 0115
---

# Contact Migration with the Web Interface

This guide walks you through configuring and starting a contact migration job using the BTSX web interface.

## Prerequisites

- BTSX web application running (see [Installation](../installation/local/) or use a [hosted instance](https://btsx.nz)).
- Source Contact Account details (NextCloud or Google).
- Destination Contact Account details (NextCloud or Google).
- Both contact servers must be accessible from the public internet.

## Configuring a Contact Migration Job
![Contact Migration Start Page](screenshots/contacts-start.png)

### Migration Type

At the top of the page, select **Contacts** from the **Migration Type** dropdown menu:

![Contact Migration Type Selection](screenshots/contacts-migration-type.png)

### Source & Destination Account Configuration

Fill in the source & destination account details.
First, select the ***Server Type*** which will dictate how authentication is performed.

#### NextCloud Servers
NextCloud provides contact storage via CardDAV protocol with password-based authentication.
With a NextCloud server, you need to enter the details as below:

1. **Server**: NextCloud server hostname (e.g., `cloud.example.com`).
2. **Username**: Your NextCloud username or email address.
3. **Password**: Your account password.
  - Use the "peak" toggle![peak toggle](screenshots/peak-toggle.png) to view the password in clear text.

![NextCloud details](screenshots/nextcloud-details.png)

Once you have entered this information, click the **Authenticate** button.
The system will attempt to authenticate with the provided details and check that they are correct.
You will not be able to start the migration until this step has completed.<br/>
![NextCloud authentication](screenshots/nextcloud-auth-success.png)

#### Google Contacts with OAuth
Google provides contact storage via the Google People API and uses OAuth-based authentication.
To migrate Google contacts, select **Google** from the **Server Type** dropdown.<br/>
![Google contacts authentication](screenshots/google-contacts-authentication.png)

Then use the **Authenticate with Google** button to invoke the OAuth authentication process.
You will be redirected to Google's login page to login using your Google account credentials.
After a successful login, you will be redirected back to the Contact Migration Tool and the **Authenticated** tick will be displayed.</br>
![Google Contacts success](screenshots/google-contacts-auth-success.png)

## Contact Migration Options
There are several options that can be configured to customize how the migration will occur as described below.
![Contact migration options](screenshots/contacts-migration-options.png)

- **Duplicate Handling**
  - Controls how the system handles contacts that already exist in the destination.
  - The check is done by comparing contact identifiers and key fields.
  - Available options:
    - **Merge**: Merges duplicate contacts by combining information from both source and destination. Use this when you want to preserve existing contact data while adding new information from the source.
    - **Skip**: Skips copying contacts that already exist in the destination. This is useful for resuming interrupted migrations or syncing only new contacts.
    - **Create Duplicate**: Copies all contacts regardless of whether they exist in the destination. This will create duplicate contacts if a migration has previously been performed. Use this only when the destination is empty or you intentionally want duplicates.
  - If a Migration Job is restarted, the Duplicate Handling will default to **Skip** to avoid unnecessary work and ensure duplicates are not created.

- **Delete Source**
  - When selected, contacts will be deleted from the Source Account after they have been copied to the Destination Account.
  - If you are only taking a backup, leave this option unchecked.

- **Import Collected Contacts**
  - When selected, the system will also migrate automatically collected contacts (contacts that were automatically added from email interactions).
  - This option is only applicable when migrating from Google Contacts.
  - Leave this option unchecked if you only want to migrate manually added contacts.

- **Import Folder Name**
  - Specifies the name of the contact folder/address book on the destination server where contacts will be imported.
  - If the folder does not exist, it will be created.
  - Leave blank to import contacts into the default address book.

## Starting the Migration

Before starting the migration, you must agree to the Terms Of Service.

If both the source and destination accounts are authenticated and the Terms of Service are agreed, the **Start Migration** button will be enabled.
Click the **Start Migration** button.
The system will start the migration job running and direct you to the Status page, where you can monitor the job.</br>

![Start Contact Migration Button](screenshots/contacts-start-button.png)
