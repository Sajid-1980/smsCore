var modul = (function () {
    $(function () {
        if (Model.ShowSession) //if class controls exist then load class data to dropdown list.
        {
             LoadSessions();
        }

        if (Model.ShowCampus && userTypes == EnumManager.BasicUserType.User) //if class controls exist then load class data to dropdown list.
        {
            
                LoadCampus();
            
        }
        else if (userTypes == EnumManager.BasicUserType.Campus) {
            {
                if (Model.ShowClasses) //if class controls exist then load class data to dropdown list.
                {
                    
                        LoadClasses();
                    
                }

                if (Model.ShowExams) //if exam controls exist then load exam data to dropdown list.
                {
                    
                        LoadExams($('#Campus').val());
                    
                } //end exam id check

                if (Model.ShowEmployee) {
                    
                        LoadEmployees($('#Campus').val());
                    

                }
            }
        }
        else if (userTypes == EnumManager.BasicUserType.Employee) {
            {
                if (Model.ShowClasses) //if class controls exist then load class data to dropdown list.
                {
                    
                        LoadClasses('ClaimHelper.GetIdFromClaims()');
                    
                }

                if (Model.ShowExams) //if exam controls exist then load exam data to dropdown list.
                {
                    
                        LoadExams($('#Campus').val());
                    
                } //end exam id check
            }
        }
        if (Model.ShowClasses && userTypes == EnumManager.BasicUserType.User) {
            
                LoadClasses();

            
        }
    }); //end body load

    if (Model.AddNew || Model.AddNewExam) {
        
            $(function() {
                $(''+Model.ParentContainerID+'' + '.new').on('click',
                    function (e) {
                        e.preventDefault();
                        var title = $(this).attr('title');
                        var url = $(this).attr('href');
                        var type = $(this).data('type');

                        BootstrapDialog.show({
                            title: title,
                            message: $('<div />').load(url),
                            onhide: function (dialogRef) {
                                if (type == 'cmp') {
                                    LoadCampus();
                                } else if (type == 'cls' || type == 'sec' || type == 'fgrp' || type == 'ses') {
                                    LoadClasses();
                                } else if (type == 'ses') {
                                    LoadSessions();
                                }
                            }
                        });
                        return false;
                    });
                });
        
    }

    if (Model.ShowSession) {
        
            function LoadSessions() {
                $.get('/api/selector/sessions?search=' + 'Model.ForSearch',
                    function (session) {
                        var sessions = JSON.parse(session);
                        var sessionCombo = $(''+Model.ParentContainerID+'#Session').html('').select2({
                            data: sessions,
                            width: '100%'
                        });

                        if (Model.SessionID != -1) {
                            
                                sessionCombo.val('Model.SessionID').trigger('change');
                            
                        }
                        $(''+Model.ParentContainerID+'#invisible-control').children('#Session').remove();
                    });
                }
        
    }//END OF SESSION LOAD

    if (Model.ShowCampus && userTypes == EnumManager.BasicUserType.User) {
        
            function LoadCampus() {
                $.get('/api/selector/campuses?search=' + 'Model.ForSearch',
                    function (campus) {
                        var campuses = JSON.parse(campus);
                        let length =(Model.ForSearch ? 2 : 1);
                        if (campuses.length == length) {

                            let cmpHtml = '<input type="hidden" value="CurrentUser.GetCampusIds().FirstOrDefault()" name="Model.CampusFieldName" id="Campus" />';
                            $(''+Model.ParentContainerID+'#invisible-control').children('#Campus').remove();
                            $(''+Model.ParentContainerID+'#campus-control').remove();
                            $(''+Model.ParentContainerID+'#invisible-control').append(cmpHtml);
                            let campusId = $(''+Model.ParentContainerID+'#Campus').val();
                            if (Model.ShowEmployee) {
                                

                                    LoadEmployees(campusId);

                                
                            }
                            if (Model.ShowSections || Model.ShowSubjects) {
                                
                                    $(''+Model.ParentContainerID+'#Classes').trigger('change');;

                                
                            }

                            if (Model.ShowExams) {
                                
                                    LoadExams(campusId);
                                
                            }
                        }
                        else {
                            var campusCombo = $(''+Model.ParentContainerID+'#Campus').html('').select2({
                                data: campuses,
                                width: '100%'
                            });

                            var combo = $(''+Model.ParentContainerID+'#Campus:not(.bound)');

                            if (Model.ShowEmployee) {
                                
                                    combo.addClass('bound').on('change',
                                    function() {
                                        LoadEmployees($(this).val());
                                        });

                                
                            }
                            if (Model.ShowSections || Model.ShowSubjects) {
                                
                                    combo.addClass('bound').on('change',
                                    function() {
                                        $(''+Model.ParentContainerID+'#Classes').trigger('change');;
                                        });

                                
                            }

                            if (Model.ShowExams) {
                                
                                    combo.addClass('bound').on('change',
                                    function() {
                                        LoadExams($(this).val());
                                        });
                                
                            }

                            if (Model.CampusID != -1) {
                                
                                    campusCombo.val('Model.CampusID').trigger('change');
                                
                            }
                            else {
                                
                                    campusCombo.trigger('change');
                                
                            }

                            $(''+Model.ParentContainerID+'#invisible-control').children('#Campus').remove();
                        }
                        (string.IsNullOrEmpty(Model.CampusLoadCallBack) ? null : Html.Raw(Model.CampusLoadCallBack))

                    });
                }
        
    }

    if (Model.ShowClasses) {
        
            function LoadClasses(employeeId = -1) {
                $.get('/api/selector/classes/Model.ForSearch' + '?employeeId=' + employeeId + '&classteacher=Model.ShowClasseForClassTeacher',
                    function (classes) {
                        var classes = JSON.parse(classes);
                        var classCombo = $(''+Model.ParentContainerID+'#Classes').html('').select2({
                            data: classes,
                            width: '100%'
                        });
                        if (Model.ClassID != -1) {
                            
                                classCombo.val('Model.ClassID').trigger('change');
                            
                        }
                        var combo = $(''+Model.ParentContainerID+'#Classes:not(.bound)');
                        $(''+Model.ParentContainerID+'#invisible-control').children('#Classes').remove();
                        if ($(''+Model.ParentContainerID+'#section-control').length > 0) //if group controls exist then load exam data to dropdown list.
                        {
                            //$('#myButton:not(.bound)').addClass('bound')
                            combo.addClass('bound').on('change',
                                function () {
                                    if (userTypes == EnumManager.BasicUserType.Employee) {
                                        
                                            LoadSections($(this).val(), 'ClaimHelper.GetIdFromClaims()');
                                        
                                    }
                                    else {
                                         LoadSections($(this).val());
                                    }
                                });
                        }
                        if ($(''+Model.ParentContainerID+'#feeGroup-control').length > 0) //if group controls exist then load exam data to dropdown list.
                        {
                            combo.addClass('bound').on('change',
                                function () {
                                    LoadFeeGroups($(this).val());
                                });

                        }
                        if ($(''+Model.ParentContainerID+'#subject-control').length > 0) //if subject controls exist then load exam data to dropdown list.
                        {
                            combo.addClass('bound').on('change',
                                function () {

                                    if (userTypes == EnumManager.BasicUserType.Employee) {
                                        
                                            LoadSubjects($(this).val(), 'ClaimHelper.GetIdFromClaims()');
                                        
                                    }
                                    else {
                                         LoadSubjects($(this).val());
                                    }
                                });

                        }

                        if ($(''+Model.ParentContainerID+'#group-control').length > 0) //if group controls exist then load exam data to dropdown list.
                        {
                            //load group like pre medical and engineering
                        }

                        classCombo.trigger('change');

                    });
                }
        
    }

    if (Model.ShowSections) {
        
            function LoadSections(classid, employeeId = -1) {
                    var campusId = $('#Campus').val();
            $.get('/api/selector/sections/' + classid + '/' + campusId + '?search=' + 'Model.ForSearch' + '&employeeId=' + employeeId+ '&classteacher=Model.ShowClasseForClassTeacher',
            function(classes) {
                            var classes = JSON.parse(classes);

            var sectionCombo = $(''+Model.ParentContainerID+'#Sections').html('').select2({
                data: classes,
            width: '100%'
                            });
            if (Model.SectionID != -1)
            {
                
                    sectionCombo.val('Model.SectionID').trigger('change');
                
            }
            $(''+Model.ParentContainerID+'#invisible-control').children('#Sections').remove();
                        });
                }
        
    }

    if (Model.ShowSubjects) {
        
            function LoadSubjects(classid, employeeId = -1) {
                    var campusId = $(''+Model.ParentContainerID+'#Campus').val();
            $.get('/api/selector/subjects/' + classid + '/' + campusId + '?search=' + 'Model.ForSearch' + '&employeeId=' + employeeId,
            function(subject) {
                            var subjects = JSON.parse(subject);

            var subjectCombo = $(''+Model.ParentContainerID+'#Subjects').html('').select2({
                data: subjects,
            width: '100%'
                            });
            if (Model.SubjectID != -1)
            {
                
                    subjectCombo.val('Model.SubjectID').trigger('change');
                
            }
            $(''+Model.ParentContainerID+'#invisible-control').children('#Subjects').remove();
                        });
                }
        
    }

    if (Model.ShowFeeGroup) {
        
            function LoadFeeGroups(classid) {
                    var campusId = $('#Campus').val();
            $.get('/api/selector/feegroups/' + classid + '/' + campusId + '?search=' + 'Model.ForSearch'+ '&showFree=' + 'Model.ShowFreeFeeGroup',
            function(feegroup) {
                            var feegroups = JSON.parse(feegroup);

            var feegroupCobmo = $(''+Model.ParentContainerID+'#FeeGroup').html('').select2({
                data: feegroups,
            width: '100%'
                            });
            if (Model.CampusID != -1)
            {
                
                    feegroupCobmo.val('Model.FeeGroupID').trigger('change');
                
            }
            $(''+Model.ParentContainerID+'#invisible-control').children('#FeeGroup').remove();
                        });
                }
        
    }

    if (Model.ShowExams) {
        
            function LoadExams(campusId) {
                $.get('/api/selector/exams/Model.ForSearch/' + campusId + '?loadlist=Model.ShowExamsList',
                    function (exam) {
                        var exams = JSON.parse(exam);

                        var examCombo = $(''+Model.ParentContainerID+'#Exams').html('').select2({
                            data: exams,
                            width: '100%'
                        });
                        if (Model.ExamID != -1) {
                            
                                examCombo.val('Model.ExamID').trigger('change');
                            
                        }
                        $(''+Model.ParentContainerID+'#invisible-control').children('#Exams').remove();
                    });
                }
        
    }

    if (Model.ShowEmployee) {
        
            function LoadEmployees(campusId) {
                $.get('/api/selector/employee/Model.ForSearch/' + campusId,
                    function (exam) {
                        var employees = JSON.parse(exam);

                        var empCombo = $(''+Model.ParentContainerID+'#Employee').html('').select2({
                            data: employees,
                            width: '100%'
                        });
                        if (Model.EmployeeID != -1) {
                            
                                empCombo.val('Model.EmployeeID').trigger('change');
                            
                        }
                        $(''+Model.ParentContainerID+'#invisible-control').children('#Employee').remove();
                    });
                }
        
    }

})(); //end of moule