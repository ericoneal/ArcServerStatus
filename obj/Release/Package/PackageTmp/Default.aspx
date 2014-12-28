<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ArcServerStatus.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>

        <link href="Content/Site.css" rel="stylesheet" type="text/css" />
    <link href="Content/themes/metroblue/jquery-ui.css" rel="stylesheet"
        type="text/css" />
    <!-- jTable style file -->
    <link href="Scripts/jtable/themes/metro/blue/jtable.css" rel="stylesheet"
        type="text/css" />
    <script src="Scripts/modernizr-2.6.2.js" type="text/javascript"></script>
    <script src="Scripts/jquery-1.9.0.min.js" type="text/javascript"></script>
    <script src="Scripts/jquery-ui-1.9.2.min.js" type="text/javascript"></script>

    <script src="Scripts/jtablesite.js" type="text/javascript"></script>
    <!-- A helper library for JSON serialization -->
    <script type="text/javascript" src="Scripts/jtable/external/json2.js"></script>
    <!-- Core jTable script file -->
    <script type="text/javascript" src="Scripts/jtable/jquery.jtable.js"></script>
    <!-- ASP.NET Web Forms extension for jTable -->
    <script type="text/javascript" src="Scripts/jtable/extensions/jquery.jtable.aspnetpagemethods.js"></script>

    <style>
        .child-opener-image
        {
            cursor: pointer;
        }
        .child-opener-image-column
        {
            text-align: center;
        }
        .jtable-dialog-form
        {
            min-width: 220px;
        }
        .jtable-dialog-form input[type="text"]
        {
            min-width: 200px;
        }
    </style>


</head>
<body>
      <div class="site-container">
        <div id="ServiceTableContainer">
        </div>
    </div>
    <script type="text/javascript">

        $(document).ready(function () {

            $('#ServiceTableContainer').jtable({
                title: 'CIRRUS Service Status',
                //paging: false, //Enable paging
                //sorting: false, //Enable sorting
                //defaultSorting: 'ServiceName ASC',
                openChildAsAccordion: true, //Enable this line to show child tabes as accordion style
                actions: {
                    listAction: 'Default.aspx/StatReport',
                    //deleteAction: '/MasterChild.aspx/DeleteStudent',
                    //updateAction: '/MasterChild.aspx/UpdateStudent',
                    //createAction: '/MasterChild.aspx/CreateStudent'
                },
                fields: {
                    OID: {
                        //  key: true,
                        // create: false,
                        //  edit: false,
                        list: false
                    },

                    Folder: {
                        title: 'Folder',
                        width: '20%'
                    },


                    ServiceName: {
                        key: true,
                        title: 'ServiceName',
                        width: '20%'
                    },

                    configuredState: {
                        title: 'Configured State',
                        width: '20%',
                        //display: 
                        //       function (data) {
                        //           {
                                    
                        //               //$('tr:has(td:contains("STARTED"))').css('background-color', 'red').css('color', '#fff');
                        //               $('tr:has(td:contains("STOPPED"))').css('background-color', 'gray').css('color', '#fff');
                        //               return data.record.configuredState;
                        //           }             
                        //       }
                            
                    },

          

                    realTimeState: {
                        title: 'Current State',
                        width: '20%'
                    },

                 
                    OptionsForm: {
                        title: '',
                        width: '5%',
                        sorting: false,
                        edit: false,
                        create: false,
                        display: function (data) {
                            //Create an image that will be used to open child table
                            var $img = $('<img src="images/options.png" title="Service Options" />');
                            //Open child table when user clicks the image
                            $img.click(function () {
                           
                    
                                opendialog();
                            

                                function opendialog() {
                                    var $dialog = $('#dialog')
                                    //.html('<iframe style="border: 0px; " src="' + page + '" width="100%" height="100%"></iframe>')
                                    .dialog({
                                        title: "Service Options",
                                        autoOpen: false,
                                        dialogClass: 'dialog_fixed,ui-widget-header',
                                        modal: true,
                                        height: 250,
                                        //minWidth: 400,
                                        minHeight: 225,
                                        draggable: true,
                                        /*close: function () { $(this).remove(); },*/
                                        buttons: { "Ok": function () { ServiceOptions($("input[name=radOption]:checked").val(), data.record.Folder, data.record.ServiceName, data.record.type); $(this).dialog("close"); } }
                                    });
                                    $dialog.dialog('open');
                                }

               
                     

                            });
                            //Return image to show on the person row
                            return $img;
                        }
                 

                },



                    //CHILD TABLE DEFINITION FOR "PHONE NUMBERS"
                    Logs: {
                        title: '',
                        width: '5%',
                        sorting: false,
                        edit: false,
                        create: false,
                        display: function (data) {
                            //Create an image that will be used to open child table
                            var $img = $('<img src="images/logDown.png" title="View Log Messages" />');
                            //Open child table when user clicks the image
                            $img.click(function () {
                                $('#ServiceTableContainer').jtable('openChildTable',
                                        $img.closest('tr'),
                                        {
                                            title: data.record.ServiceName + ' - Logs',
                                            actions: {
                                                listAction: 'Default.aspx/GetLogs?strFoldername=' + data.record.Folder + "&strServicename=" + data.record.ServiceName + "&strType=" + data.record.type
                                                //deleteAction: '/Demo/DeletePhone',
                                                //updateAction: '/Demo/UpdatePhone',
                                                //createAction: '/Demo/CreatePhone'
                                            },
                                            fields: {
                                                code: {
                                                    title: 'Code',
                                                    width: '20%'
                                                },
                                                message: {
                                                    title: 'Message',
                                                    width: '20%'
                                                }
                                               
                                            },

                                            //rowInserted: function (event, data2) {
                                            //    // alert(data);
                                            //    if (data2.record.code != 0) {
                                            //       // data2.row.css('background-color', 'gray');
                                            //        var row = $('#ServiceTableContainer').jtable('getRowByKey', data.record.ServiceName);
                                            //        row.css('background-color', 'red');
                                            //    }
                                                
                                            //}

                                        },
                                        function (data) { //opened handler
                                            data.childTable.jtable('load');
                                        });



                            });
                            //Return image to show on the person row
                            return $img;
                        }
                    }



                },


                rowInserted: function (event, data) {
                    //alert(data);
                    data.row.css('background-color', 'PaleGreen');
                    data.row.css('font-weight', 'bold');


                    if (data.record.configuredState == "STOPPED") {
                        data.row.css('background-color', 'gray');
                    }

                    if ((data.record.configuredState == "STARTED") && (data.record.realTimeState == "STOPPED")) {
                        data.row.css('background-color', 'FireBrick');
                    }

                    if (data.record.hasError == "True") {
                        data.row.css('background-color', 'FireBrick');
                    }

                }
            });

            //Load person list from server
            $('#ServiceTableContainer').jtable('load');

        
        });

        function ServiceOptions(val, folder, servicename, type) {
            // alert(val);
            $("#ServiceTableContainer").jtable('showBusy', 'Calling ArcServer...');

            var parameters = '{strAction: "' + val + '", strFoldername: "' + folder + '", strServicename: "' + servicename + '", strType: "' + type + '"}';


            $.ajax({
                type: "POST",
                url: "Default.aspx/ServiceOptions",
                data: parameters,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    var resp = JSON.parse(response.d);
                    var ServerResponse = "";
                    try{
                        ServerResponse = (resp.messages[0]);
                    }
                    catch(ex)
                    {
                        ServerResponse = (resp.status);
                    }
          
                    $("#ServiceTableContainer").jtable('hideBusy');
                    $('#ServiceTableContainer').jtable('reload', RefreshCallback(ServerResponse));

                    //var jtable = $('#ServiceTableContainer').jtable;
                    //jtable.reload(RefreshCallback(ServerResponse));

                }

            });
        }


        function RefreshCallback(ServerResponse)
        {
            alert(ServerResponse);
        }
      
    </script>


    <div id="dialog" style="display:none">

        <div id="ui-id-7" class="ui-dialog-content ui-widget-content" style="width: auto; min-height: 26px; max-height: none; height: auto;">
         <form id="jtable-edit-form" class="jtable-dialog-form jtable-edit-form">
             <input type="hidden" name="StudentId" id="Edit-StudentId" value="20" />
            <div class="jtable-input-field-container">
                <div class="jtable-input-label">Power Option</div>
                <div class="jtable-input jtable-radiobuttonlist-input">
                    <div class="jtable-radio-input">
                        <input type="radio" id="radOption-0" name="radOption" value="start" checked="true">
                        <span class="jtable-option-text-clickable">Start</span>
                    </div>
                    <div class="jtable-radio-input">
                        <input type="radio"  id="radOption-1" name="radOption" value="stop" >
                        <span class="jtable-option-text-clickable">Stop</span>
                    </div>
        <%--            <div class="jtable-radio-input">
                        <input type="radio" id="radOption-2" name="radOption" checked="true" value="restart" >
                        <span class="jtable-option-text-clickable">Restart</span>
                    </div>--%>


                </div>

            </div>
            </form>
     
     </div>



    </div>


</body>
</html>
