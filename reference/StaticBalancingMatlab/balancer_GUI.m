function varargout = balancer_GUI(varargin)
% BALANCER_GUI MATLAB code for balancer_GUI.fig
%      BALANCER_GUI, by itself, creates a new BALANCER_GUI or raises the existing
%      singleton*.
%
%      H = BALANCER_GUI returns the handle to a new BALANCER_GUI or the handle to
%      the existing singleton*.
%
%      BALANCER_GUI('CALLBACK',hObject,eventData,handles,...) calls the local
%      function named CALLBACK in BALANCER_GUI.M with the given input arguments.
%
%      BALANCER_GUI('Property','Value',...) creates a new BALANCER_GUI or raises the
%      existing singleton*.  Starting from the left, property value pairs are
%      applied to the GUI before balancer_GUI_OpeningFcn gets called.  An
%      unrecognized property name or invalid value makes property application
%      stop.  All inputs are passed to balancer_GUI_OpeningFcn via varargin.
%
%      *See GUI Options on GUIDE's Tools menu.  Choose "GUI allows only one
%      instance to run (singleton)".
%
% See also: GUIDE, GUIDATA, GUIHANDLES

% Edit the above text to modify the response to help balancer_GUI

% Last Modified by GUIDE v2.5 14-Oct-2020 11:36:07

% Begin initialization code - DO NOT EDIT
gui_Singleton = 1;
gui_State = struct('gui_Name',       mfilename, ...
                   'gui_Singleton',  gui_Singleton, ...
                   'gui_OpeningFcn', @balancer_GUI_OpeningFcn, ...
                   'gui_OutputFcn',  @balancer_GUI_OutputFcn, ...
                   'gui_LayoutFcn',  [] , ...
                   'gui_Callback',   []);
if nargin && ischar(varargin{1})
    gui_State.gui_Callback = str2func(varargin{1});
end

if nargout
    [varargout{1:nargout}] = gui_mainfcn(gui_State, varargin{:});
else
    gui_mainfcn(gui_State, varargin{:});
end
% End initialization code - DO NOT EDIT


% --- Executes just before balancer2_GUI is made visible.
function balancer_GUI_OpeningFcn(hObject, eventdata, handles, varargin)
% This function has no output args, see OutputFcn.
% hObject    handle to figure
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
% varargin   command line arguments to balancer2_GUI (see VARARGIN)

% Choose default command line output for balancer2_GUI
handles.output = hObject;

% Update handles structure
guidata(hObject, handles);

% UIWAIT makes balancer2_GUI wait for user response (see UIRESUME)
% uiwait(handles.figure1);
    
try
    appdata('load',matpath);
    UpdateGUI(handles);
catch ME
    switch ME.identifier
        case 'MATLAB:load:couldNotReadFile'
            disp('No data loaded')
            beep;
            
            default = getdefault;
            appdata('set',default);
        otherwise
            rethrow(ME);
    end
end


% --- Outputs from this function are returned to the command line.
function varargout = balancer_GUI_OutputFcn(hObject, eventdata, handles) 
% varargout  cell array for returning output args (see VARARGOUT);
% hObject    handle to figure
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Get default command line output from handles structure
varargout{1} = handles.output;


% --- Executes on button press in Button_System.
function Button_System_Callback(hObject, eventdata, handles)
% hObject    handle to Button_System (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

%get serialnumber and model from user
default = appdata('get','default');
currentsystem = appdata('get','gantry');
% if isempty(currentsystem) || ~currentsystem.hasmodel
%     currentsystem = default;
% end
newsystem = getsystem(currentsystem);
if isempty(newsystem) || isequal(newsystem,currentsystem)
    return
end

%a change will be made
erase = questdlg('Erase old data?','Erase?','Erase','Export','Keep','Erase');
switch erase
    case 'Erase'
        resetconfig;
    case 'Export'
        exportlog;
        resetconfig;
end
appdata('set','gantry',newsystem)
UpdateGUI(handles);


% --- Executes on button press in Button_Calibrate.
function Button_Calibrate_Callback(hObject, eventdata, handles)
% hObject    handle to Button_Calibrate (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
    
    gantry = appdata('get','gantry');
    
    if isempty(gantry) || ~gantry.hasmodel
        disp('Missing System Information');
        beep;
        return
    end
    if gantry.haslog
        defaultpath = gantry.log(1).filepath;
    elseif gantry.hascal
        defaultpath = gantry.cal.filename{1};
    else
        defaultpath = '';
    end
    
    %get calibration data paths
    [filepath, filename,baseCWstate] = getrawcaldata(gantry,defaultpath);
    calibratedgantry = gantry.calibrate(filepath,filename,baseCWstate.count);
    
    notes=inputdlg('Add Comments','Notes',[2 110]);
    if isempty(notes) || strcmp(notes{1},'')
        notes{1} = ' ';
    end
    calibratedgantry = calibratedgantry.setnotes(notes,1);
    
    appdata('set','gantry',calibratedgantry);
    
    UpdateGUI(handles);
    waitfor(msgbox({'Calibration Complete!';'Return to baseline configuration.'},'Instruction','help'));



% --- Executes on button press in Button_Close.
function Button_Close_Callback(hObject, eventdata, handles)
% hObject    handle to Button_Close (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
    default = appdata('get','default');
    appdata('save',matpath);
    appdata('clear');
    close;


% --- Executes on button press in Button_Measure.
function Button_Measure_Callback(hObject, eventdata, handles)
% hObject    handle to Button_Measure (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
gantry = appdata('get','gantry');
if isempty(gantry)
    disp('Missing System Information');
    beep;
    return
end

if gantry.haslog
    defaultpath = [gantry.log(1).filepath gantry.log(1).filename];
    defaultCWcount = gantry.log(1).CWstate.count;
else
    default = getappdata(0,'default');
    defaultpath = default.balancedatapath;
    defaultCWcount = systemclass.typicalCWcount;
end

[filename filepath] = uigetfile(gantry.importformat,'Provide data file',defaultpath);
CWcount = getCWcount(gantry.position,gantry.weight,defaultCWcount);
CWstate = CWstateclass(gantry.position,gantry.weight,CWcount);
newgantry = gantry.addmeasurement(filepath,filename, CWstate);

notes=inputdlg('Add Comments','Notes',[2 110]);
    if isempty(notes) || strcmp(notes{1},'')
        notes{1} = ' ';
    end
newgantry = newgantry.setnotes(notes,1);

appdata('set','gantry',newgantry);

UpdateGUI(handles);


% --- Executes when selected cell(s) is changed in Balance_Table.
function Balance_Table_CellSelectionCallback(hObject, eventdata, handles)
% hObject    handle to Balance_Table (see GCBO)
% eventdata  structure with the following fields (see UITABLE)
%	Indices: row and column indices of the cell(s) currently selecteds
% handles    structure with handles and user data (see GUIDATA)    
selcel = eventdata.Indices;

appdata('set','selectedcell',selcel)
if isempty(selcel)
    row = [];
else
    row = selcel(1);
end

gantry = appdata('get','gantry');
UpdateSelected(handles,gantry.log,row)


% --- Executes on button press in Button_ExportLog.
function Button_ExportLog_Callback(hObject, eventdata, handles)
% hObject    handle to Button_ExportLog (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
exportlog;

% --- Executes on button press in Button_ImportLog.
function Button_ImportLog_Callback(hObject, eventdata, handles)
% hObject    handle to Button_ImportLog (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
resetconfig;
importlog;
UpdateGUI(handles);


function UpdateGUI(h)
% updates all graphics and redraws GUI
default = appdata('get','default');
gantry = appdata('get','gantry');
if gantry.hasmodel
    UpdateSystemText(h.System_Text,gantry);
else
    set(h.System_Text, 'String', ' ');
end

if gantry.hascal
    UpdateCalibrationText(h.Calibration_Text, gantry);
    PlotCalibrationVectors(h.Calibrate_Axes,gantry,gantry.cal,default);
else
    set(h.Calibration_Text, 'String', ' '); 
    cla(h.Calibrate_Axes);
    legend(h.Calibrate_Axes, 'off');
end

if gantry.haslog
    UpdateLogTable(h.Balance_Table,gantry,gantry.log);
else
    set(h.Balance_Table,'ColumnName',[]);
    set(h.Balance_Table,'Data',[]);
end

selectedcell = appdata('get','selectedcell');
if ~isempty(selectedcell)
    UpdateSelected(h,gantry.log,selectedcell(1));
else
    drawnow;
end    

    
function UpdateSelected(h,log,row)

if isempty(log) || isempty(row) || row > length(log)
    set(h.Balance_Text1, 'String', '');
    set(h.Balance_Text2, 'String', '');
    set(h.Balance_Text3, 'String', '');
    cla(h.Balance_Axes)
else
    UpdateLogText(h.Balance_Text1, h.Balance_Text2, h.Balance_Text3, log(row));
    PlotBalanceData(h.Balance_Axes,log,row);
end
drawnow;
    

function UpdateSystemText(htextbox, gantry)
% creates string for System_Text
    text = sprintf('%s\n%s\n%3.1f [g*m]\n%0.0f [deg]\n', gantry.serial, gantry.model, gantry.maximbalance, gantry.hometickoffset);

    position = gantry.position;
    for i=1:5
        if i>length(position)
            text = sprintf('%s\n',text);
        else
            text = sprintf('%s%s, %3.0f , %3.0f\n', text, position(i).name, position(i).radius, position(i).angle);
        end
    end

    weight = gantry.weight;
    for i=1:length(weight)
        text = sprintf('%s%s, %2.1f, %1.1f\n', text, weight(i).name, weight(i).mass, weight(i).thickness);
    end
    
    set(htextbox, 'String', text);


function UpdateCalibrationText(htextbox, gantry)
% creates string for Calibration_Text
    cal=gantry.cal;
    position=gantry.position;
    transform = mat2text('%0.2e',gantry.cal.sig2posTransform);
    text1 = sprintf('%2.1f [rpm]\n%s\n%s',cal.fitvector(3,1),transform);

    text2='';
    for i=1:length(position)
        calibrationweight = nonzeros(gantry.calCWcount{i}*[gantry.weight.mass]');
        text2 = sprintf('%s(%s)\n%2.1f [kg]\n[%1.2e, %1.2e] [g*m]\n%2.1f [%%]\n%2.1f [deg]\n\n',text2,upper(position(i).name),calibrationweight,cal.impactvector(i,1),cal.impactvector(i,2),cal.DCerr(i),cal.angleerr(1));
    end
    textf = sprintf('%s%s',text1,text2);
    set(htextbox, 'String', textf);

    
    function UpdateLogTable(huitable,gantry,log)
    position = gantry.position;
    
    if ~gantry.haslog
        table.ColumnName = {};
        table.ColumnWidth = {};
        table.Data = [];
    end
    
    pos=3;
    table.ColumnName = cell(1,pos+length(position));
    table.ColumnName(1:pos) = {'Timestamp','Imbalance [g*m]','Angle [deg]'};
    table.ColumnWidth = cell(size(table.ColumnName));
    table.ColumnWidth(1:pos) = {120,100,80};
    
    for i=1:length(position)
        table.ColumnName{pos+i} = sprintf('Dweight(%s) [kg]',position(i).name);
        table.ColumnWidth{pos+i} = 130;
    end
    for i=1:length(log)
        tableline = {log(i).timestamp, sprintf('%0.0f',log(i).imbmag), sprintf('%0.0f',log(i).imbdir)};
        for j = 1:length(position)
            tableline = [tableline, sprintf('%0.1f',log(i).dCW(j))]; %#ok<AGROW> variable must grow in loop. length is unknown.
        end
        table.Data(i,:) = tableline;
    end
    
    set(huitable,'ColumnName',table.ColumnName);
    set(huitable,'Data',table.Data);
    set(huitable,'ColumnWidth',table.ColumnWidth);
    

function UpdateLogText(htextbox1, htextbox2, htextbox3, selLog)
% generates text data based on selected log file
    text1 = sprintf('%s\n%s\n',selLog.passfail,[selLog.filepath selLog.filename]);
    CWcount = selLog.CWstate.count;
    for j=1:size(CWcount,1)
        for i=1:size(CWcount,2)
            if i==1
                text1 = sprintf('%s[%0.0f',text1,CWcount(j,i));
            else
                text1 = sprintf('%s, %0.0f',text1,CWcount(j,i));
            end
        end
        text1 = sprintf('%s]\n',text1);
    end
    
    text2 = sprintf('%0.1f\n%0.1f\n%0.0f',selLog.fitvector(3),selLog.SNR,selLog.force);
    
    text3 = selLog.notes(1,:);
    for i=2:size(selLog.notes,1)
        text3 = sprintf('%s\n%s',text3,selLog.notes(i,:));
    end
    
    set(htextbox1, 'String', text1);
    set(htextbox2, 'String', text2);
    set(htextbox3, 'String', text3);
    
    
function PlotCalibrationVectors(fig,systemclass, cal, default)
% generates plot for Axes_Calibration
    
    %set up
    R=max(vecnorm(cal.impactvector)); %radius
    S=4;   %num circ.lines
    N=16;   %num ang.lines

    sect_width=2*pi/N;    
    offset_angle=0:sect_width:2*pi-sect_width;

    r=linspace(0,R,S+1);
    w=0:.01:2*pi;

    hold(fig,'on')
    axis(fig, 'equal')

    %add background image
    image('Parent',fig,'CData',default.rotorimage,'XData',[-R, R],'YData',[R, -R]);

    %add polar grid
    for n=2:length(r)
        plot(fig,real(r(n)*exp(1i*w)),imag(r(n)*exp(1i*w)),'k--');
    end
    for n=1:length(offset_angle)
        plot(fig,real([0 R]*exp(1i*offset_angle(n))),imag([0 R]*exp(1i*offset_angle(n))),'k-');
    end

    %add vectors
    p=plot(fig,[0; cal.impactvector(2,1)],[0; cal.impactvector(1,1)],[0; cal.impactvector(2,2)],[0; cal.impactvector(1,2)],'LineWidth',4);
    legend(p,{systemclass.position(1).name,systemclass.position(2).name});

    % hide axes
    set(fig,'visible','off');
   
    % using compass plot instead 
%     p=compass(fig,cal.impactvector(1,:),cal.impactvector(2,:));
% 
%     colors = get(0,'DefaultAxesColorOrder');
%     for i=1:length(p)
%         set(p(i),'color',colors(mod(i-1,length(colors))+1,:),'linewidth',4)
%     end
%     camorbit(0,180)
%     camroll(90)
%     
%     hold(fig,'on')
%     image('Parent',fig,'CData',default.rotorimage,'XData',[-R, R],'YData',[R, -R]);

   

function PlotBalanceData(fig,log,row)
% generates plot for Balance_Axes
    cla(fig);

    if ~isempty(log)
        selLog = log(row);

        hold(fig,'on')

        sinfunc = @(x) selLog.fitvector(1)*cos(x) + selLog.fitvector(2)*sin(x) + selLog.fitvector(3);
        colormap winter
        colors = linspace(0,1,length(selLog.angle))';
        scatter(fig,selLog.angle,selLog.signal,2,colors);
        plot(fig,sort(selLog.angle),sinfunc(sort(selLog.angle*pi/180)),'-r','LineWidth',3);
        
        set(fig,'XLim',[0 360])
        set(fig,'XTick',[90 180 270])
    end


function [CWcount] = getCWcount(position,weight,suggestedCWcount)
%walks user through the data aquisition needed for counterweight count
    CWcount = suggestedCWcount;

    i=1;
    while i < length(position)
        text = strcat({'Part Number:'},{weight.name},{'      Mass:'},cellstr(num2str([weight.mass]'))',{'[kg]     Thickness:'},cellstr(num2str([weight.thickness]'))',' [mm]');
        values = inputdlg(text,['Number of counterweights @ ' position(i).name 'position:'],[1 80],sprintfc('%d',CWcount(i,:)));
        newvalue = (str2double(values))';
        if any(isnan(newvalue))
            waitfor(warndlg('Unable to interpret inputs. Please try again.','Warning'));
            continue
        end
        CWcount(i,:) = newvalue;
        i=i+1;
    end




function [filepath, filename,CWstate] = getrawcaldata(gantry,defaultdatapath)
%walks user through the data aquisition needed for calibration.
    position = gantry.position;
    weight = gantry.weight;
    dataformat = gantry.importformat;

    waitfor(msgbox({'             CALIBRATION PROCESS';...
                    ' ';...
                    '1) Hand balance. Ensure rotation is SAFE.';...
                    ' ';...
                    '2) Record the count of each counterweight.';...
                    '     Take a "baseline" measurement.';...
                    ' ';...
                    '3) Add calibration weight(s) and take measurement.';...
                    '     Remove calibration weight(s).';...
                    ' ';...
                    '4) Repeat step (3) for all positions.'},'Instruction','help'));
    
    CWcount = getCWcount(position,weight,gantry.typCWcount);
    CWstate = CWstateclass(position,weight,CWcount);
    
    filename = cell(1,length(position)+1);
    filepath = cell(size(filename));
    if isempty(defaultdatapath)
        [filename{1} filepath{1}] = uigetfile(dataformat,'Baseline file.');
    else
        [filename{1} filepath{1}] = uigetfile(dataformat,'Baseline file.',defaultdatapath);
    end
    for i=2:length(position)+1
        [filename{i} filepath{i}] = uigetfile(dataformat,['Add ' position(i-1).name ' calibration weight and measure balance.'],filepath{1});
    end
    
    
function [systemout] = getsystem(gantry)
% requests model and serial number information through GUI and updates
% gantry class accordingly
    default = appdata('get','default');
    if isempty(gantry) || isempty(gantry.serial)
        gantry.serial = default.serial;
    end

    defindx = 1;
    [indx,tf] = listdlg('PromptString',{'Select Model'},'SelectionMode','single',...
                        'ListString',default.modellist,'InitialValue',defindx);
    if ~tf
        return
    end
    systemout = GantryBalancingClass(default.modellist{indx});

    input = gantry.serial;
    checksout = false;
    while checksout == false
        input = char(inputdlg({'SerialNumber:'},'Inputs',[1 35],{input}));
        if isempty(input)
            answer = questdlg('Are you sure you want to start a log without a serial number?');
            if strcmp(answer,'Yes')
                return
            else
                continue
            end
        end
        if length(input)~=12
            waitfor(warndlg('Valid serial numbers must be 12 charecters long.'));
            continue
        end
        serialnumber = input;
        checksout = true;
    end
    systemout = systemout.setserial(serialnumber);
    

function resetconfig
%erases gantry data
    try
        rmappdata(0,'gantry'); 
    catch ME
        switch ME.identifier
            case 'MATLAB:HandleGraphics:Appdata:InvalidNameAppdata'
            otherwise
                rethrow(ME);
        end
    end
    

function exportlog
% saves guide and default variables
    [file,path] = uiputfile('*.mat','Export Data');
    if isempty(file)
        return
    end
    
    try
        gantry = getappdata(0,'gantry'); %#ok<NASGU> gantry variable is used in save command
        save([path file],'gantry','default','-mat');
    catch ME
        disp('Failed to export data.')
        beep;
        rethrow(ME); % add cases later
    end
    
    
function importlog
% loads new gantry file
	[file, path] = uigetfile('*.mat','Import Data');
    if isempty(file)
        return
    end
    
    try
        S = load([path file],'-mat');
        resetconfig;
        setappdata(0,'gantry',S.gantry);
        setappdata(0,'defualt',S.default);
    catch ME
        disp('Failed to import data');
        beep;
        rethrow(ME); %add cases later
    end


function [default] = getdefault()
% loads default values
    default.serial = 'ABC123999999';
    %default.balancedatapath = 'P:\BalanceLogs\';
    default.rotorimage = imread('Generic CT Rotor Image.PNG');
    default.modellist = {'CT16','CT64','CT256','CT256TEMP'};

    
function [p] = matpath()
%returns location of saved variable data
    p = 'config.mat';
    
