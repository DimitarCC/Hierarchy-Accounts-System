I. Run the project
    1. Install the following software:
     - Visual Studio (version 2022 or later)
     - Micrsoft .NET (version 8.0.x)
     - SQL Server Express or normal (version 2019 or later)

    2. Open the solution file (.slnx) in Visual Studio.

    3. Restore NuGet packages by right-clicking on the solution in Solution Explorer and selecting "Restore NuGet Packages". Or rebuild the solution.

    4. Update the database connection string in the appsettings.json file located in the api project directory to point to your SQL Server instance and desired database name. The api will create the database automatically if it does not exist on first run.
    4.a. There is initial seeding of Global account so you dont need to create  it manually.

    5. Run the app in debug or release mode by pressing F5 or clicking the "Start" button in Visual Studio.
    5.a. You can deploy the builded app to dedicated IIS server if needed and run it from there.



II. How to test the API endpoints using Swagger UI
    1. After build and run the solution you will be automatically redirected to the swagger page where you can test all the endpoints.
    1.a. If you have installed the API on dedicated IIS server navigate in the browser to <Host>/swagger to access the swagger page

    2. Pick your desired endpoint that you want to test and click on "Try It Out" button.

    3. Once that is done you will be presented with UI where to enter Query string parameters or payload (JSON format) where required

    4. Every endpoint is documented so if more information is needed can be looked at descriptions.

III. Testing via Postman or other external tools
    1. Open Postman application or any other similar tool.
    2. Create a new request by clicking on "New" and selecting "Request".
    3. Set the request method (GET, POST, PUT, DELETE, etc.) and enter the API endpoint URL (e.g., http://localhost:5000/api/your-endpoint).
    4. If the endpoint requires parameters or a request body, add them in the appropriate sections.
    5. Click on "Send" to execute the request.
    6. Review the response status code, headers, and body returned by the API.

  SAMPLE REQUESTS:
    1. Get single account details
    - GET
    - <Host>/api/v1/account/get?accountId=<accountId_for_witch_we_want_data> (if you specify account id)
    - <Host>/api/v1/account/get (to get the master root account)

    2. Get account tree details
    - GET
    - <Host>/api/v1/account/gettree?accountId=<accountId_for_witch_we_want_data> (if you specify account for which you want to get the subtree)
    - <Host>/api/v1/account/gettree (to get subtree for the master root account)

    3. Add new child/root account
    - POST
    - <Host>/api/v1/account/add
    - payload (JSON)
        {
          "ParentId": 0, // ommit this to add master root account
          "Name": "Account A"
        }

    4. Move account to different parrent
    - PUT
    - <Host>/api/v1/account/move?accountId=<accountId_for_moving>&newParentId=<accountId_of_new_parent>

    5. Delete account and promote subtree
    - DELETE
    - <Host>/api/v1/account/delete?accountId=<accountId_for_deleting>

IV. Testing via the written unit tests
    1. Unit tests are located in the Tests project.

    2. To run the tests, open the Test Explorer in Visual Studio (Test > Test Explorer) and click on "Run All" to execute all tests.

    3. Review the test results in the Test Explorer window.