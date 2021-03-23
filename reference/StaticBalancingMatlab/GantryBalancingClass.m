classdef GantryBalancingClass
    %UNTITLED Summary of this class goes here
    %   Detailed explanation goes here
    
    properties (Constant, Hidden)     
        catalog = struct('model',         {'CT16',    'CT64',    'CT256', 'CT256TEMP'},...
                         'importtemplate',{'yaskawa', 'yaskawa', 'akd',   'akd'},...
                         'importformat',  {'*.txt',   '*.txt',   '*.csv', '*.csv'},...
                         'hometickoffset',{80,        80,        18,      0},... %deg
                         'maximbalance',  {50,        50,        572.9,   572.9},... %g*m
                         'maxspeed',      {120,       180,       240,     240},... %rpm
                         'position',{[balpositionclass('6 O''clock',780,180,200,'Axial'), balpositionclass('9 O''clock',800,270,200,'Axial')],... %CT16
                                     [balpositionclass('6 O''clock',780,180,200,'Axial'), balpositionclass('9 O''clock',800,270,200,'Axial')],... %CT64
                                     [balpositionclass('5 O''clock',830,165,100,'Radial-'), balpositionclass('7 O''clock',830,195,100,'Radial-')],... %CT256
                                     [balpositionclass('5 O''clock',830,165,100,'Radial-'), balpositionclass('7 O''clock',830,195,100,'Radial-')]},... %CT256TEMP
                         'weight',{[counterweightclass('16-1000',2.7,10), counterweightclass('16-1001',1.2,5), counterweightclass('16-1002',0.5,1), counterweightclass('Calibration',1.1,0)],... %CT16
                                   [counterweightclass('16-1000',2.7,10), counterweightclass('16-1001',1.2,5), counterweightclass('16-1002',0.5,1), counterweightclass('Calibration',1.1,0)],... %CT64
                                   [counterweightclass('16-3241',1.7,6), counterweightclass('16-3242',0.7,3), counterweightclass('16-4282',0.3,1), counterweightclass('16-4283',0.2,1)],... %CT256
                                   [counterweightclass('16-3241',1.7,0), counterweightclass('16-3242',0.7,0), counterweightclass('16-4282',0.3,0), counterweightclass('16-4283',0.2,0)]},... %CT256TEMP
                         'typCWcount',{[ 5, 0, 2, 0;...
                                         7, 1, 0, 0],... %CT16
                                       [ 5, 0, 2, 0;...
                                         7, 1, 0, 0],... %CT64
                                       [19, 0, 0, 0;...
                                        22, 0, 1, 1],... %CT256
                                       [19, 0, 0, 0;...
                                        22, 0, 1, 1]},... %CT256TEMP           
                         'calCWcount',{{[ 0,  0, 0, 1;...
                                          0,  0, 0, 0],...
                                        [ 0,  0, 0, 0;...
                                          0,  0, 0, 1]},... %CT16
                                       {[ 0,  0, 0, 1;...
                                          0,  0, 0, 0],...
                                        [ 0,  0, 0, 0;...
                                          0,  0, 0, 1]},... %CT64
                                       {[ 2,  0, 0, 0;...
                                          0,  0, 0, 0],...
                                        [ 0,  0, 0, 0;...
                                          2,  0, 0, 0]},... %CT256
                                       {[-2,  0, 0, 0;...
                                          0,  0, 0, 0],...
                                        [ 0,  0, 0, 0;...
                                         -2,  0, 0, 0]}}); %CT256TEMP
    end
    
    properties (Hidden)
        importtemplate
    end
    
    properties
        hasmodel = false;
        model
        importformat
        position
        weight
        hometickoffset
        maximbalance
        maxspeed
        typCWcount
        calCWcount
        
        hasserial = false;
        serial
        
        hascal = false;
        cal
%         caldata_filename
%         caldata_filepath
%         caldata_fitvector
%         cal_impactvector
%         cal_ivangle
%         cal_ivmagnitude
%         cal_sig2posTransform
%         cal_pos2imbTransform
%         cal_DCerr
%         cal_angleerr
        
        haslog = false;
        log
%         logdata_filepath
%         logdata_filename
%         logdata_timestamp
%         logdata_angle
%         logdata_signal
%         logdata_fitvector
%         log_fvSNR
%         log_CWstate
%         log_imbalancevector
%         log_positionvector
%         log_ivmagnitude
%         log_ivangle
%         log_force
%         log_passfail
%         log_deltaCW
%         log_notes
    end
    
     methods
         function obj = GantryBalancingClass(varargin)
             % constructor
             %
             % input1 = string defining gantry model number
             % input2 (option) = string defining serial number

             model = varargin{1};         
             
             if ~ischar(model)
                 error('Invalid input')
             end
             
             ind = find(ismember({obj.catalog.model}, upper(model)));
             if isempty(ind)
                 error(sprintf('%s ','Model not cataloged. Cataloged Models are:', obj.catalog.model{:}));
             end
             
             obj.model =          obj.catalog(ind).model;
             obj.importtemplate = obj.catalog(ind).importtemplate;
             obj.importformat =   obj.catalog(ind).importformat;
             obj.hometickoffset = obj.catalog(ind).hometickoffset;
             obj.maximbalance =   obj.catalog(ind).maximbalance;
             obj.maxspeed =       obj.catalog(ind).maxspeed;
             obj.position =       obj.catalog(ind).position;
             obj.weight =         obj.catalog(ind).weight;
             obj.typCWcount =     obj.catalog(ind).typCWcount;
             obj.calCWcount =     obj.catalog(ind).calCWcount;
             obj.hasmodel = true;
             
             if nargin > 1
                 SN = varargin{2};
                 obj = obj.setserial(SN);
             end
         end
         
         
         function obj = setserial(obj,SN)
             % sets the value of the "serial" field if input is valid
             if ~ischar(SN)
                 error('Invalid input');
             end
             if length(SN) ~= 12
                 error('Serial numbers must be 12 charecters long.')
             end
             
             obj.serial = SN;
             obj.hasserial = true;
         end
         
         
         function obj = calibrate(obj,filepath,filename,baseCWcount)
             % filepath and filename are cellstring arrays of the length of
             % the positions + 1. The first cell represents the baseline
             % case (no calibration weights). The remaining cells represent
             % the data when calibration weight has been added for
             % position 1, postion 2, position 3, ...
             %
             % This function uses the supplied data to build an
             % understanding of how changes in the applied imbalance
             % influence the input signal.
             if ~obj.hasmodel
                 error('System model must be defined in order to calibrate.')
             end
             if length(filepath) ~= length(obj.position)+1 || length(filepath) ~= length(filename)
                 error('Incorrect number of file paths');
             end
             if size(baseCWcount) ~= size(obj.typCWcount)
                 error('Incorrectly sized input for counterweight count.');
             end
             
             % get data from files and calculate the change in the input
             % signal ("b") for each test case.
             try
                 [fitvector(:,1),angle{1},signal{1}] = obj.readBalanceData([filepath{1} filename{1}]);
                 for i=1:length(obj.position)
                     [fitvector(:,i+1),angle{i+1},signal{i+1}] = obj.readBalanceData([filepath{i+1} filename{i+1}]);
                     fv_diff(:,i) = fitvector(:,i+1)-fitvector(:,1);
                 end
             catch ME
                 switch ME.identifier
                     case 'MATLAB:FileIO:InvalidFid'
                         error('Unable to find file')
                     otherwise
                         rethrow(ME);
                 end
             end
             
             % "b" is a vector which represents the change to the input
             % signal in terms of sin (global-Y) and cos (global-X). "B" is
             % a matrix of the "b" vectors for each calibration weight.
             B = fv_diff(1:2,:); %removes DC component (row 3)
             if rank(B) ~= 2
                 error('Sinasoid vectors are not rank 2. Verify that inputs are not singular.');
             end
             
                          
             % determine amount of applied imbalance and calculate the
             % change to the imbalance for each test case.
             CWstate(1) =       CWstateclass(obj.position,obj.weight,baseCWcount);
             for i=1:length(obj.position)
                 CWstate(i+1) = CWstateclass(obj.position,obj.weight,baseCWcount + obj.calCWcount{i});
                 imb_diff(:,i) = CWstate(i+1).appliedimbalance - CWstate(1).appliedimbalance;
             end
             
             % "x" is a vector which represents the change to the applied
             % imbalance (AKA: amount of counterweight) at each position.
             % "X" is a matrix of the "x" vectors for each calibration
             % weight.
             X = imb_diff;
             if rank(X) ~= 2
                 error('Imbalance vectors are not rank 2. Verify that inputs are not signular.');
             end
             
             % "A" is matrix representing the transformation from "x" to
             % "b". (Ax = b)
             A = B/X;
             
             % The basis of "x" allows us to solve for exactly how much
             % weight must be applied at each position, but it is also
             % helpful to visualize the system imbalance in the intuitive
             % global-X vs. global-Y coordinate system. Therefore, we 
             % create "Cx = d" where "d" is the imbalance with respect to
             % XY and "C" is the transform from "x" (position basis) to "d"
             % (XY basis).
             % As "b" and "x" already have the correct direction, we will
             % normalize the vectors in each to represent our new "x" and
             % "d" basis
             C = mtrxnorm(B)'*mtrxnorm(X);
             
             obj.cal.filename = filename;
             obj.cal.filepath = filepath;
             obj.cal.fitvector = fitvector;
             obj.cal.impactvector = C*X;
             obj.cal.sig2posTransform = A;
             obj.cal.pos2imbTransform = C;
             for i=1:length(obj.position)
                 [obj.cal.ivmagnitude(i), obj.cal.ivangle(i)] = magdir(obj.cal.impactvector(:,i));
                 obj.cal.DCerr(i) = 100*(fitvector(3,i+1) - fitvector(3,1))/fitvector(3,1);
                 obj.cal.angleerr(i) = constrainangle(obj.cal.ivangle(i) - obj.position(i).angle,-180,180);
             end
             obj.hascal = true;
             
             % add baseline measurment to log as entry
             obj = addmeasurement(obj,filepath{1}, filename{1}, CWstate(1));
         end    
         
         
         function obj = addmeasurement(obj,filepath, filename, CWstate)
             % In calibration, 2 relationships were defined:
             %     Ax=b
             %     Cx=d
             % where x = a vector representing the imbalance at each
             %           position.
             %       b = a vector representing the input signal in global-X
             %           and -Y coordinates
             %       d = a vector represting the imbalance in global-X and
             %           -Y coordinates
             %       A,C are the transforms defined between these values 
             %           during calibration
             %
             % given known values of x and b, from previous measurements
             % ("x0" and "b0") the required change ("dx" and "db") to
             % balance the system can be calculated as follows:
             %
             %       A(dx) = db
             % The system will be balanced when db = -b0.
             %       A(dx) = -b0
             %          dx = A\(-b0)
             %
             % This assumes the system is not overconstrained (more places
             % to put weight than the minimium 2). If overconstrained, the
             % system can be solved using a least squares approach:
             %
             %       dx = (A'*A)\A'*(-b0)
             if ~obj.hasmodel
                 error('System model must be defined in order to take measurements.')
             end
             if ~obj.hascal
                 error('System must be calibrated in order to take measurements.')
             end
             
             % get data from files
             try
                 [fitvector,angle,signal] = obj.readBalanceData([filepath filename]);
             catch ME
                 switch ME.identifier
                     case 'MATLAB:FileIO:InvalidFid'
                         error('Unable to find file')
                     otherwise
                         rethrow(ME);
                 end
             end
             
             % solve system for x and d
             b0 = fitvector(1:2);
             A = obj.cal.sig2posTransform;
             C = obj.cal.pos2imbTransform;
             
             if (length(A) > 2)
                 dx = (A'*A)\A'*-b0;
             else
                 dx = A\-b0;
             end
             dd = C*dx;
             
             % dividing the imbalance at each position by that positions
             % radius finds the required change in counterweight
             dCW = dx/CWstate.activeradius;
             
             % Generate remaining log data
             fileinfo=dir([filepath filename]);
             SNR = sig2noiseratio(signal,fitvector(1).*cos(angle.*pi/180) + fitvector(2).*sin(angle.*pi/180) + fitvector(3)); %dB
             
             newlog.filename = filename;
             newlog.filepath = filepath;
             newlog.timestamp = fileinfo.date;
             newlog.angle = angle;
             newlog.signal = signal;
             newlog.fitvector = fitvector;
             newlog.SNR = SNR;
             newlog.CWstate = CWstate;
             newlog.imbXYvector = dd;
             newlog.imbposvector = dx;
             newlog.dCW = dCW;
             [newlog.imbmag, newlog.imbdir] = magdir(dd);
             newlog.force = (newlog.imbmag*(obj.maxspeed*pi/30)^2)/1000; %N
             if newlog.imbmag < obj.maximbalance
                 newlog.passfail = 'PASS';
             else
                 newlog.passfail = 'FAIL';
             end
             newlog.notes={' '};
             
             obj.log = [newlog, obj.log];
             obj.haslog = true;
         end
         
         function [fitvector,angle,data] = readBalanceData(obj,cfilepath)
             [angle,data] = balancer_importdata(cfilepath, obj.importtemplate);
             angle = constrainangle(angle + obj.hometickoffset,0,360);
             fitvector = sinefit(angle*pi/180,data)';
         end
         
         function obj = setnotes(obj,notes,ind)
             %sets the value of the notes field for the log defined by ind.
             obj.log(ind).notes = notes;
         end
         
         function obj = removedata(obj,fields,ind)
             switch fields
                 case 'model'
                     obj.model='';
                     obj.importtemplate='';
                     obj.importformat='';
                     obj.position={};
                     obj.weight={};
                     obj.hometickoffset=[];
                     obj.maximbalance=[];
                     obj.maxspeed=[];
                     obj.typCWcount=[];
                     obj.calCWcount={};
                     obj.hasmodel = false;
                 case 'serial'
                     obj.serial='';
                     obj.hasserial = false;
                 case 'calibration'
                     obj.cal={};
                     obj.hascal=false;
                 case 'log'
                     obj.log={};
                     obj.haslog=false;
                 case 'row'
                     obj.log(ind) = [];
             end
         end
     end
     
end


function [angle,data] = balancer_importdata(loc, template)
    switch template
        case 'yaskawa'
            fid = fopen(loc);
            data = textscan(fid,'%*s %*s %*s %*s %f, %*s %*s %f, %*s');
            fclose(fid);

            angle=data{1,2};
            data=data{1,1};
        case 'akd'
            fid = fopen(loc);  
            data = textscan(fid,'%*s %f %*s %f %*[^\n]','Delimiter',',','HeaderLines',1);
            fclose(fid);

            angle=data{1,1};
            data=data{1,2};
    end
end