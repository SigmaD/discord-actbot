//require the discord.js module
const Discord = require('discord.js');
//Create a new discord client
const client = new Discord.Client();

//express here
const express = require('express');
const bodyParser = require('body-parser');
const app = express();
app.use(bodyParser.urlencoded({ extended: true }));

//Load config options
const { prefix, token, allowed_userid_arr, act_guid, express_port } = require('./config.json');

//When the discord client is ready, run this code
client.once('ready', () => {
	console.log('Discord Client Ready!');
});

//vars for persistent message tracking for parse
let msg;
let parseFlag = false;

client.on('message', message => {
	//Print message to log
	//console.log(message.author.username+": "+message.content);
	
	//Check for allowed userid
	if (allowed_userid_arr.includes(message.author.id)){
		//console.log('auth user');
		//Message author is authorized
		if (message.content === `${prefix}ping`){
			//send back pong in the channel the message was sent
			message.channel.send('Pong');
		}else if (message.content === `${prefix}user-info`){
			message.channel.send(`Your username: ${message.author.username}\nYour ID: ${message.author.id}`);
			console.log(message.author.id);
		}else if (message.content === `${prefix}parse`){
			message.channel.send('Ready').then(sentMessage => {
				console.log('Starting parsing');
				msg = sentMessage;
				parseFlag = true;
			});
		}else if (message.content === `${prefix}endparse`){
			console.log('Stopping parsing');
			parseFlag = false;
			delete msg;
		}
	}
});

//Login to discord with the app token
client.login(token);

//Express app functionality
app.post('/', (req, res) => {
	//console.log("Received a POST call");
	if (req.body.guid == act_guid){
		//Check if parsing
		if (parseFlag){
			//Received a valid ACT POST call
			//Check discord output has content
			if (req.body.log !== ""){
				msg.edit(req.body.log);
				console.log(req.body);
				res.send('connected: parsing to discord');
			}else {
				console.log("No log data");
				res.send('Error: missing log');
			}
		}else {
			res.send('connected: not parsing');
		}
	}
});

//Start listening
app.listen(express_port, () =>{
	console.log('app listening on 3000');
});